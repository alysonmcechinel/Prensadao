using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prensadao.Application
{
    public class RabbitMqConfig
    {
        private readonly IModel _channel;

        public RabbitMqConfig(IModel channel)
        {
            _channel = channel;
        }        

        public Task Config()
        {
            // exchanges
            _channel.ExchangeDeclare(RabbitMqConstants.Exchanges.OrderExchange, ExchangeType.Fanout, durable: true);

            // queues
            _channel.QueueDeclare(RabbitMqConstants.Queues.OrderCozinhaQueue, durable: true);
            _channel.QueueDeclare(RabbitMqConstants.Queues.OrderNotifyQueue, durable: true);

            // bind queues to exchange
            _channel.QueueBind(RabbitMqConstants.Queues.OrderCozinhaQueue, RabbitMqConstants.Exchanges.OrderExchange, "");
            _channel.QueueBind(RabbitMqConstants.Queues.OrderNotifyQueue, RabbitMqConstants.Exchanges.OrderExchange, "");

            return Task.CompletedTask;
        }
    }
}
