using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using ChineseSaleApi.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChineseSaleApi.Services
{
    public interface IKafkaProducerService
    {
        Task PublishAsync(string message, string? topic = null);
    }

    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly IProducer<Null, string> _producer;
        private readonly KafkaSettings _settings;
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(IProducer<Null, string> producer, IOptions<KafkaSettings> settings, ILogger<KafkaProducerService> logger)
        {
            _producer = producer;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task PublishAsync(string message, string? topic = null)
        {
            try
            {
                var targetTopic = topic ?? _settings.Topic;
                var result = await _producer.ProduceAsync(targetTopic, new Message<Null, string> { Value = message });
                _logger.LogInformation("Published message to {Topic} at {Partition}:{Offset}", targetTopic, result.Partition, result.Offset);
            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError(ex, "Failed to publish message to Kafka topic.");
                throw;
            }
        }
    }
}
