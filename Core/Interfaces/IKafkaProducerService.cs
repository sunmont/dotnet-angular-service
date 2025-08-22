using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTOs;


namespace Core.Interfaces
{
    public interface IKafkaProducerService
    {
        Task ProduceOrderAsync(KafkaOrderDto order);
        Task ProduceNotificationAsync(KafkaNotificationDto notification);
        Task ProduceAsync<T>(string topic, string key, T message) where T : class;
    }
}
