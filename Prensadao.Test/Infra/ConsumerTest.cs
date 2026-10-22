using System.Text;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Prensadao.Application.DTOs;
using Prensadao.Infra.Messaging.Interfaces;
using Prensadao.Infra.Messaging.RabbitMq;
using RabbitMQ.Client;

namespace Prensadao.Test.Infra;

public class ConsumerTest
{
    [Fact]
    public async Task Listen_ShouldAckMessage_WhenHandlerSucceeds()
    {
        // arrange
        const string queue = "test.queue";
        const ulong deliveryTag = 10;
        var channel = A.Fake<IModel>();
        var consumer = CreateConsumer(channel);
        var messageHandled = false;
        var rabbitConsumer = CaptureRabbitConsumer(channel, queue);

        await consumer.Listen<OrderMessageDto>(
            queue,
            message =>
            {
                messageHandled = message.OrderId == 123;
                return Task.CompletedTask;
            },
            CancellationToken.None);

        // act
        await DeliverAsync(rabbitConsumer.Value, deliveryTag, "{\"OrderId\":123}");

        // assert
        Assert.True(messageHandled);
        A.CallTo(() => channel.BasicAck(deliveryTag, false)).MustHaveHappenedOnceExactly();
        A.CallTo(() => channel.BasicNack(A<ulong>._, A<bool>._, A<bool>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Listen_ShouldNackWithoutRequeue_WhenHandlerFails()
    {
        // arrange
        const string queue = "test.queue";
        const ulong deliveryTag = 20;
        var channel = A.Fake<IModel>();
        var consumer = CreateConsumer(channel);
        var rabbitConsumer = CaptureRabbitConsumer(channel, queue);

        await consumer.Listen<OrderMessageDto>(
            queue,
            _ => throw new InvalidOperationException("Falha esperada no teste."),
            CancellationToken.None);

        // act
        await DeliverAsync(rabbitConsumer.Value, deliveryTag, "{\"OrderId\":123}");

        // assert
        A.CallTo(() => channel.BasicNack(deliveryTag, false, false)).MustHaveHappenedOnceExactly();
        A.CallTo(() => channel.BasicAck(A<ulong>._, A<bool>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Listen_ShouldNackWithoutRequeue_WhenPayloadDeserializesToNull()
    {
        // arrange
        const string queue = "test.queue";
        const ulong deliveryTag = 30;
        var channel = A.Fake<IModel>();
        var consumer = CreateConsumer(channel);
        var rabbitConsumer = CaptureRabbitConsumer(channel, queue);

        await consumer.Listen<OrderMessageDto>(queue, _ => Task.CompletedTask, CancellationToken.None);

        // act
        await DeliverAsync(rabbitConsumer.Value, deliveryTag, "null");

        // assert
        A.CallTo(() => channel.BasicNack(deliveryTag, false, false)).MustHaveHappenedOnceExactly();
        A.CallTo(() => channel.BasicAck(A<ulong>._, A<bool>._)).MustNotHaveHappened();
    }

    private static Consumer CreateConsumer(IModel channel)
    {
        var config = A.Fake<IRabbitMqConfig>();
        A.CallTo(() => config.CreateChannel()).Returns(channel);
        A.CallTo(() => channel.IsOpen).Returns(true);

        return new Consumer(config, A.Fake<ILogger<Consumer>>());
    }

    private static CapturedConsumer CaptureRabbitConsumer(IModel channel, string queue)
    {
        var captured = new CapturedConsumer();

        A.CallTo(() => channel.BasicConsume(
                queue,
                false,
                string.Empty,
                false,
                false,
                A<IDictionary<string, object>>._,
                A<IBasicConsumer>._))
            .Invokes(call => captured.Value = (IAsyncBasicConsumer)call.GetArgument<IBasicConsumer>(6))
            .Returns("test-consumer");

        return captured;
    }

    private static Task DeliverAsync(IAsyncBasicConsumer consumer, ulong deliveryTag, string json)
        => consumer.HandleBasicDeliver(
            "test-consumer",
            deliveryTag,
            false,
            string.Empty,
            string.Empty,
            A.Fake<IBasicProperties>(),
            Encoding.UTF8.GetBytes(json));

    private sealed class CapturedConsumer
    {
        public IAsyncBasicConsumer Value { get; set; } = null!;
    }
}
