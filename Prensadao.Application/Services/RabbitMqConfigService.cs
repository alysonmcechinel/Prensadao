using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prensadao.Application.Services
{
    public class RabbitMqConfigService : IRabbitMqConfigService
    {
        private readonly IConnection _connection;

        public RabbitMqConfigService()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connection = factory.CreateConnection("prensadao-app");
        }

        public IModel CreateChannel() => _connection.CreateModel();

        public Task Config()
        {
            using var channel = _connection.CreateModel();

            // exchanges
            channel.ExchangeDeclare(RabbitMqConstants.Exchanges.OrderExchange, ExchangeType.Fanout, durable: true);

            // queues
            channel.QueueDeclare(RabbitMqConstants.Queues.OrderCozinhaQueue, durable: true, exclusive: false, arguments: null);
            channel.QueueDeclare(RabbitMqConstants.Queues.OrderNotifyQueue, durable: true, exclusive: false, arguments: null);

            // bind queues to exchange
            channel.QueueBind(RabbitMqConstants.Queues.OrderCozinhaQueue, RabbitMqConstants.Exchanges.OrderExchange, "");
            channel.QueueBind(RabbitMqConstants.Queues.OrderNotifyQueue, RabbitMqConstants.Exchanges.OrderExchange, "");

            return Task.CompletedTask;
        }
    }
}
