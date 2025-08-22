namespace API.Configuration
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public KafkaTopics Topics { get; set; } = new();
    }

    public class KafkaTopics
    {
        public string Orders { get; set; } = string.Empty;
        public string Notifications { get; set; } = string.Empty;
    }
}
