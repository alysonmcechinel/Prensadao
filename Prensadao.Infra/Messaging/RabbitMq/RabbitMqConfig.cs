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

            // exchanges
            channel.ExchangeDeclare(RabbitMqConstants.Exchanges.OrderExchange, ExchangeType.Fanout, durable: true);
            channel.ExchangeDeclare(RabbitMqConstants.Exchanges.NotifyExchange, ExchangeType.Fanout, durable: true);

            // queues
            channel.QueueDeclare(RabbitMqConstants.Queues.OrderCozinhaQueue, durable: true, exclusive: false, arguments: null);
            channel.QueueDeclare(RabbitMqConstants.Queues.OrderNotifyQueue, durable: true, exclusive: false, arguments: null);

            // bind queues to exchange
            channel.QueueBind(RabbitMqConstants.Queues.OrderCozinhaQueue, RabbitMqConstants.Exchanges.OrderExchange, "");
            channel.QueueBind(RabbitMqConstants.Queues.OrderNotifyQueue, RabbitMqConstants.Exchanges.NotifyExchange, "");

            return Task.CompletedTask;
        }

        public void Dispose() => _connection.Dispose();
    }
}
