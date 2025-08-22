namespace Core.DTOs
{
    public class KafkaNotificationDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
