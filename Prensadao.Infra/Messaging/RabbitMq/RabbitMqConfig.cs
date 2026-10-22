using Prensadao.Application;
using Prensadao.Infra.Messaging.Interfaces;
using RabbitMQ.Client;

namespace Prensadao.Infra.Messaging.RabbitMq
{
    public class RabbitMqConfig : IRabbitMqConfig
    {
        private readonly IConnection _connection;

        public RabbitMqConfig()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection("prensadao-app");
        }

        public IModel CreateChannel() => _connection.CreateModel();

        public Task Config()
        {
            using var channel = _connection.CreateModel();
            RabbitMqTopology.Configure(channel);

            return Task.CompletedTask;
        }

        public void Dispose() => _connection.Dispose();
    }
}
