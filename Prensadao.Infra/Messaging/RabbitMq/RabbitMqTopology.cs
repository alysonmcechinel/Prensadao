using Prensadao.Application;
using RabbitMQ.Client;

namespace Prensadao.Infra.Messaging.RabbitMq;

public static class RabbitMqTopology
{
    public static void Configure(IModel channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        DeclareExchanges(channel);
        DeclareDeadLetterQueues(channel);
        DeclareApplicationQueues(channel);
        BindQueues(channel);
    }

    private static void DeclareExchanges(IModel channel)
    {
        channel.ExchangeDeclare(RabbitMqConstants.Exchanges.OrderExchange, ExchangeType.Fanout, durable: true);
        channel.ExchangeDeclare(RabbitMqConstants.Exchanges.OrderStatusChangedExchange, ExchangeType.Fanout, durable: true);
        channel.ExchangeDeclare(RabbitMqConstants.Exchanges.DeadLetterExchange, ExchangeType.Direct, durable: true);
    }

    private static void DeclareDeadLetterQueues(IModel channel)
    {
        channel.QueueDeclare(
            RabbitMqConstants.Queues.OrderCozinhaDeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        channel.QueueDeclare(
            RabbitMqConstants.Queues.OrderStatusMessageDeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    private static void DeclareApplicationQueues(IModel channel)
    {
        channel.QueueDeclare(
            RabbitMqConstants.Queues.OrderCozinhaQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: CreateDeadLetterArguments(RabbitMqConstants.RoutingKeys.OrderCozinhaDeadLetterRoutingKey));

        channel.QueueDeclare(
            RabbitMqConstants.Queues.OrderStatusMessageQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: CreateDeadLetterArguments(RabbitMqConstants.RoutingKeys.OrderStatusMessageDeadLetterRoutingKey));
    }

    private static void BindQueues(IModel channel)
    {
        channel.QueueBind(RabbitMqConstants.Queues.OrderCozinhaQueue, RabbitMqConstants.Exchanges.OrderExchange, "");
        channel.QueueBind(
            RabbitMqConstants.Queues.OrderStatusMessageQueue,
            RabbitMqConstants.Exchanges.OrderStatusChangedExchange,
            "");

        channel.QueueBind(
            RabbitMqConstants.Queues.OrderCozinhaDeadLetterQueue,
            RabbitMqConstants.Exchanges.DeadLetterExchange,
            RabbitMqConstants.RoutingKeys.OrderCozinhaDeadLetterRoutingKey);
        channel.QueueBind(
            RabbitMqConstants.Queues.OrderStatusMessageDeadLetterQueue,
            RabbitMqConstants.Exchanges.DeadLetterExchange,
            RabbitMqConstants.RoutingKeys.OrderStatusMessageDeadLetterRoutingKey);
    }

    private static IDictionary<string, object> CreateDeadLetterArguments(string routingKey)
        => new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = RabbitMqConstants.Exchanges.DeadLetterExchange,
            ["x-dead-letter-routing-key"] = routingKey
        };
}
