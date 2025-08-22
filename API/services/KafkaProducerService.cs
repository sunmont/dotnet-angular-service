using Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Configuration;
using Confluent.Kafka;
using Core.DTOs;


namespace Infrastructure.Services
{
    internal class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly KafkaSettings _kafkaSettings;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IOptions<KafkaSettings> kafkaSettings, ILogger<KafkaProducerService> logger)
        {
            _kafkaSettings = kafkaSettings.Value;
            _logger = logger;

            var config = new ProducerConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers,
                Acks = Acks.All,
                //Retries = 3,
                RetryBackoffMs = 1000,
                RequestTimeoutMs = 30000,
                MessageTimeoutMs = 60000
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task ProduceOrderAsync(KafkaOrderDto order)
        {
            await ProduceAsync(_kafkaSettings.Topics.Orders, order.Id.ToString(), order);
        }

        public async Task ProduceNotificationAsync(KafkaNotificationDto notification)
        {
            await ProduceAsync(_kafkaSettings.Topics.Notifications, notification.Id, notification);
        }

        public async Task ProduceAsync<T>(string topic, string key, T message) where T : class
        {
            try
            {
                var serializedMessage = JsonSerializer.Serialize(message);

                var result = await _producer.ProduceAsync(topic, new Message<string, string>
                {
                    Key = key,
                    Value = serializedMessage
                });

                _logger.LogInformation("Message delivered to topic {Topic}, partition {Partition}, offset {Offset}",
                    result.Topic, result.Partition, result.Offset);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(ex, "Failed to deliver message to topic {Topic}: {Reason}", topic, ex.Error.Reason);
                throw;
            }
        }

        public void Dispose()
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }
    }
}
