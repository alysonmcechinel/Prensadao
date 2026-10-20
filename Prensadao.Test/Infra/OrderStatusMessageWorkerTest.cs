using FakeItEasy;
using Microsoft.Extensions.Logging;
using Prensadao.Application;
using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;
using Prensadao.Infra.Messaging.Workers;

namespace Prensadao.Test.Infra;

public class OrderStatusMessageWorkerTest
{
    [Fact]
    public async Task ExecuteAsync_ShouldBuildMessage_WhenStatusProducesMessage()
    {
        // arrange
        var consumer = A.Fake<IConsumer>();
        var resolver = A.Fake<IOrderStatusMessageStrategyResolver>();
        var strategy = A.Fake<IOrderStatusMessageStrategy>();
        var logger = A.Fake<ILogger<OrderStatusMessageWorker>>();
        var message = new OrderStatusChangedMessageDto
        {
            OrderId = 10,
            Status = OrderStatusEnum.Pronto,
            IsDelivery = true
        };
        using var cancellation = new CancellationTokenSource();

        A.CallTo(() => resolver.TryResolve(message.Status, out strategy))
            .Returns(true);
        A.CallTo(() => consumer.Listen<OrderStatusChangedMessageDto>(
                RabbitMqConstants.Queues.OrderStatusMessageQueue,
                A<Func<OrderStatusChangedMessageDto, Task>>._,
                cancellation.Token))
            .ReturnsLazily(async call =>
            {
                var handler = call.GetArgument<Func<OrderStatusChangedMessageDto, Task>>(1);
                await handler(message);
                cancellation.Cancel();
            });

        var worker = new TestableOrderStatusMessageWorker(consumer, resolver, logger);

        // act
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => worker.RunAsync(cancellation.Token));

        // assert
        A.CallTo(() => strategy.BuildMessage(message)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotBuildMessage_WhenStatusValueIsInvalid()
    {
        // arrange
        var consumer = A.Fake<IConsumer>();
        var resolver = A.Fake<IOrderStatusMessageStrategyResolver>();
        var strategy = A.Fake<IOrderStatusMessageStrategy>();
        var logger = A.Fake<ILogger<OrderStatusMessageWorker>>();
        var message = new OrderStatusChangedMessageDto
        {
            OrderId = 10,
            Status = (OrderStatusEnum)999,
            IsDelivery = true
        };
        using var cancellation = new CancellationTokenSource();

        A.CallTo(() => resolver.TryResolve(message.Status, out strategy))
            .Returns(false);
        A.CallTo(() => consumer.Listen<OrderStatusChangedMessageDto>(
                RabbitMqConstants.Queues.OrderStatusMessageQueue,
                A<Func<OrderStatusChangedMessageDto, Task>>._,
                cancellation.Token))
            .ReturnsLazily(async call =>
            {
                var handler = call.GetArgument<Func<OrderStatusChangedMessageDto, Task>>(1);
                await handler(message);
                cancellation.Cancel();
            });

        var worker = new TestableOrderStatusMessageWorker(consumer, resolver, logger);

        // act
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => worker.RunAsync(cancellation.Token));

        // assert
        A.CallTo(() => strategy.BuildMessage(A<OrderStatusChangedMessageDto>._)).MustNotHaveHappened();
    }

    private sealed class TestableOrderStatusMessageWorker : OrderStatusMessageWorker
    {
        public TestableOrderStatusMessageWorker(
            IConsumer consumer,
            IOrderStatusMessageStrategyResolver resolver,
            ILogger<OrderStatusMessageWorker> logger)
            : base(consumer, resolver, logger)
        {
        }

        public Task RunAsync(CancellationToken cancellationToken) => ExecuteAsync(cancellationToken);
    }
}
