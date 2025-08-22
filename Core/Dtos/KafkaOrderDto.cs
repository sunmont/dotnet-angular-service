namespace Core.DTOs
{
    public class KafkaOrderDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
