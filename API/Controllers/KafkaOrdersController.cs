using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Core.DTOs;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IKafkaProducerService _kafkaProducer;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IKafkaProducerService kafkaProducer, ILogger<OrdersController> logger)
        {
            _kafkaProducer = kafkaProducer;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var order = new KafkaOrderDto
                {
                    Id = Random.Shared.Next(1000, 9999),
                    CustomerName = request.CustomerName,
                    ProductName = request.ProductName,
                    Amount = request.Amount,
                    OrderDate = DateTime.UtcNow
                };

                // Produce order to Kafka
                await _kafkaProducer.ProduceOrderAsync(order);

                // Also send a notification
                var notification = new KafkaNotificationDto
                {
                    Message = $"New order created for {order.CustomerName}",
                    Type = "OrderCreated"
                };

                await _kafkaProducer.ProduceNotificationAsync(notification);

                _logger.LogInformation("Order {OrderId} created and sent to Kafka", order.Id);

                return Ok(new { OrderId = order.Id, Message = "Order created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulkOrders([FromBody] List<CreateOrderRequest> requests)
        {
            try
            {
                var orderIds = new List<int>();

                foreach (var request in requests)
                {
                    var order = new KafkaOrderDto
                    {
                        Id = Random.Shared.Next(1000, 9999),
                        CustomerName = request.CustomerName,
                        ProductName = request.ProductName,
                        Amount = request.Amount,
                        OrderDate = DateTime.UtcNow
                    };

                    await _kafkaProducer.ProduceOrderAsync(order);
                    orderIds.Add(order.Id);
                }

                var notification = new KafkaNotificationDto
                {
                    Message = $"Bulk order creation completed: {orderIds.Count} orders created",
                    Type = "BulkOrderCreated"
                };

                await _kafkaProducer.ProduceNotificationAsync(notification);

                _logger.LogInformation("Bulk order creation completed: {Count} orders", orderIds.Count);

                return Ok(new { OrderIds = orderIds, Message = "Bulk orders created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk orders");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class CreateOrderRequest
    {
        public string CustomerName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
