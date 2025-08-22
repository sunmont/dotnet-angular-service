using API.Configuration;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Core.DTOs;

namespace API.services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly KafkaSettings _kafkaSettings;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public KafkaConsumerService(
            IOptions<KafkaSettings> kafkaSettings,
            ILogger<KafkaConsumerService> logger,
            IServiceProvider serviceProvider)
        {
            _kafkaSettings = kafkaSettings.Value;
            _logger = logger;
            _serviceProvider = serviceProvider;

            var config = new ConsumerConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers,
                GroupId = _kafkaSettings.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                SessionTimeoutMs = 30000,
                HeartbeatIntervalMs = 10000
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(new[] { _kafkaSettings.Topics.Orders, _kafkaSettings.Topics.Notifications });

            _logger.LogInformation("Kafka consumer started. Subscribed to topics: {Topics}",
                string.Join(", ", _kafkaSettings.Topics.Orders, _kafkaSettings.Topics.Notifications));
        
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);

                        if (consumeResult?.Message != null)
                        {
                            await ProcessMessageAsync(consumeResult);
                            _consumer.Commit(consumeResult);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message: {Reason}", ex.Error.Reason);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error in consumer loop");
                        await Task.Delay(5000, stoppingToken); // Wait before retrying
                    }
                }
            }
            finally
            {
                _consumer.Close();
            }
        }

        private async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult)
        {
            _logger.LogInformation("Received message from topic {Topic}, partition {Partition}, offset {Offset}, key {Key}",
                consumeResult.Topic, consumeResult.Partition, consumeResult.Offset, consumeResult.Message.Key);

            using var scope = _serviceProvider.CreateScope();

            try
            {
                switch (consumeResult.Topic)
                {
                    case var topic when topic == _kafkaSettings.Topics.Orders:
                        await ProcessOrderMessage(consumeResult.Message.Value, scope.ServiceProvider);
                        break;
                    case var topic when topic == _kafkaSettings.Topics.Notifications:
                        await ProcessNotificationMessage(consumeResult.Message.Value, scope.ServiceProvider);
                        break;
                    default:
                        _logger.LogWarning("Unknown topic: {Topic}", consumeResult.Topic);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from topic {Topic}", consumeResult.Topic);
                throw; // Rethrow to prevent commit
            }
        }

        private async Task ProcessOrderMessage(string messageValue, IServiceProvider serviceProvider)
        {
            var order = JsonSerializer.Deserialize<KafkaOrderDto>(messageValue);
            if (order != null)
            {
                _logger.LogInformation("Processing order: {OrderId} for customer {CustomerName}",
                    order.Id, order.CustomerName);

                // Add your business logic here
                // Example: Save to database, send email, etc.
                await Task.Delay(100); // Simulate processing time

                _logger.LogInformation("Order {OrderId} processed successfully", order.Id);
            }
        }

        private async Task ProcessNotificationMessage(string messageValue, IServiceProvider serviceProvider)
        {
            var notification = JsonSerializer.Deserialize<KafkaNotificationDto>(messageValue);
            if (notification != null)
            {
                _logger.LogInformation("Processing notification: {NotificationId} - {Message}",
                    notification.Id, notification.Message);

                // Add your notification logic here
                // Example: Send push notification, email, SMS, etc.
                await Task.Delay(50); // Simulate processing time

                _logger.LogInformation("Notification {NotificationId} processed successfully", notification.Id);
            }
        }

        public override void Dispose()
        {
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}
