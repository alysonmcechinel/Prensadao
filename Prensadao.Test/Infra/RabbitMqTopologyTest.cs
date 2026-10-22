using FakeItEasy;
using Prensadao.Application;
using Prensadao.Infra.Messaging.RabbitMq;
using RabbitMQ.Client;

namespace Prensadao.Test.Infra;

public class RabbitMqTopologyTest
{
    [Fact]
    public void Configure_ShouldDeclareAndBindDeadLetterTopology()
    {
        // arrange
        var channel = A.Fake<IModel>();

        // act
        RabbitMqTopology.Configure(channel);

        // assert
        A.CallTo(() => channel.ExchangeDeclare(
                RabbitMqConstants.Exchanges.DeadLetterExchange,
                ExchangeType.Direct,
                true,
                false,
                null))
            .MustHaveHappenedOnceExactly();

        AssertQueueUsesDeadLetterRoutingKey(
            channel,
            RabbitMqConstants.Queues.OrderCozinhaQueue,
            RabbitMqConstants.RoutingKeys.OrderCozinhaDeadLetterRoutingKey);
        AssertQueueUsesDeadLetterRoutingKey(
            channel,
            RabbitMqConstants.Queues.OrderStatusMessageQueue,
            RabbitMqConstants.RoutingKeys.OrderStatusMessageDeadLetterRoutingKey);

        AssertDeadLetterQueueIsDeclaredAndBound(
            channel,
            RabbitMqConstants.Queues.OrderCozinhaDeadLetterQueue,
            RabbitMqConstants.RoutingKeys.OrderCozinhaDeadLetterRoutingKey);
        AssertDeadLetterQueueIsDeclaredAndBound(
            channel,
            RabbitMqConstants.Queues.OrderStatusMessageDeadLetterQueue,
            RabbitMqConstants.RoutingKeys.OrderStatusMessageDeadLetterRoutingKey);
    }

    private static void AssertQueueUsesDeadLetterRoutingKey(
        IModel channel,
        string queue,
        string routingKey)
    {
        A.CallTo(() => channel.QueueDeclare(
                queue,
                true,
                false,
                false,
                A<IDictionary<string, object>>.That.Matches(arguments =>
                    (string)arguments["x-dead-letter-exchange"] == RabbitMqConstants.Exchanges.DeadLetterExchange &&
                    (string)arguments["x-dead-letter-routing-key"] == routingKey)))
            .MustHaveHappenedOnceExactly();
    }

    private static void AssertDeadLetterQueueIsDeclaredAndBound(
        IModel channel,
        string queue,
        string routingKey)
    {
        A.CallTo(() => channel.QueueDeclare(queue, true, false, false, null))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => channel.QueueBind(
                queue,
                RabbitMqConstants.Exchanges.DeadLetterExchange,
                routingKey,
                null))
            .MustHaveHappenedOnceExactly();
    }
}
