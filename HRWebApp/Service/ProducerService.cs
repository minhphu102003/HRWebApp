using AutoMapper.Configuration;
using Confluent.Kafka;
using System.Threading.Tasks;

namespace HRWebApp.Service
{
    // class này s? ???c em kh?i t?o trong personalsController.cs
    public class ProducerService
    {
        private readonly IProducer<Null, string> _producer;
        public ProducerService(string bootstrapServers)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };

            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }

        public async Task ProduceAsync(string topic, string message, int partition)
        {
            var kafkamessage = new Message<Null, string>
            {
                Value = message,
            };
            var topicPartition = new TopicPartition(topic, new Partition(partition));
            await _producer.ProduceAsync(topicPartition, kafkamessage);
        }
    }
}