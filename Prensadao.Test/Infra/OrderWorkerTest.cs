using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Prensadao.Application;
using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Enums;
using Prensadao.Domain.Repositories;
using Prensadao.Infra.Messaging.Workers;

namespace Prensadao.Test.Infra;

public class OrderWorkerTest
{
    [Fact]
    public async Task ExecuteAsync_ShouldStartPreparationAndPublishOnce_WhenCreatedMessageIsRepeated()
    {
        // arrange
        const int orderId = 10;
        var repository = A.Fake<IOrderRepository>();
        var consumer = A.Fake<IConsumer>();
        var publisher = A.Fake<IMessagePublisher>();
        var order = CreateOrder(orderId);
        A.CallTo(() => repository.GetByIdAsync(orderId)).Returns(order);

        using var provider = new ServiceCollection()
            .AddSingleton(repository)
            .BuildServiceProvider();
        using var cancellation = new CancellationTokenSource();

        A.CallTo(() => consumer.Listen<OrderMessageDto>(
                RabbitMqConstants.Queues.OrderCozinhaQueue,
                A<Func<OrderMessageDto, Task>>._,
                cancellation.Token))
            .ReturnsLazily(async call =>
            {
                var handler = call.GetArgument<Func<OrderMessageDto, Task>>(1);
                await handler(new OrderMessageDto { OrderId = orderId });
                await handler(new OrderMessageDto { OrderId = orderId });
                cancellation.Cancel();
            });

        var worker = new TestableOrderWorker(provider, consumer, publisher);

        // act
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => worker.RunAsync(cancellation.Token));

        // assert
        Assert.Equal(OrderStatusEnum.EmPreparacao, order.Status);
        A.CallTo(() => repository.UpdateAsync(order)).MustHaveHappenedOnceExactly();
        A.CallTo(() => publisher.PublishAsync(
                A<OrderStatusChangedMessageDto>.That.Matches(message =>
                    message.OrderId == orderId &&
                    message.Status == OrderStatusEnum.EmPreparacao),
                RabbitMqConstants.Exchanges.OrderStatusChangedExchange,
                A<string?>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldIgnoreDuplicateMessage_WhenOrderAlreadyStartedPreparation()
    {
        // arrange
        const int orderId = 10;
        var repository = A.Fake<IOrderRepository>();
        var consumer = A.Fake<IConsumer>();
        var publisher = A.Fake<IMessagePublisher>();
        var order = CreateOrder(orderId);
        order.AdvanceStatus();
        A.CallTo(() => repository.GetByIdAsync(orderId)).Returns(order);

        using var provider = new ServiceCollection()
            .AddSingleton(repository)
            .BuildServiceProvider();
        using var cancellation = new CancellationTokenSource();

        A.CallTo(() => consumer.Listen<OrderMessageDto>(
                RabbitMqConstants.Queues.OrderCozinhaQueue,
                A<Func<OrderMessageDto, Task>>._,
                cancellation.Token))
            .ReturnsLazily(async call =>
            {
                var handler = call.GetArgument<Func<OrderMessageDto, Task>>(1);
                await handler(new OrderMessageDto { OrderId = orderId });
                await handler(new OrderMessageDto { OrderId = orderId });
                cancellation.Cancel();
            });

        var worker = new TestableOrderWorker(provider, consumer, publisher);

        // act
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => worker.RunAsync(cancellation.Token));

        // assert
        Assert.Equal(OrderStatusEnum.EmPreparacao, order.Status);
        A.CallTo(() => repository.UpdateAsync(A<Order>._)).MustNotHaveHappened();
        A.CallTo(() => publisher.PublishAsync(
                A<OrderStatusChangedMessageDto>._,
                A<string>._,
                A<string?>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_ShouldFailMessage_WhenOrderDoesNotExist()
    {
        // arrange
        const int orderId = 999;
        var repository = A.Fake<IOrderRepository>();
        var consumer = A.Fake<IConsumer>();
        var publisher = A.Fake<IMessagePublisher>();
        A.CallTo(() => repository.GetByIdAsync(orderId)).Returns((Order)null!);

        using var provider = new ServiceCollection()
            .AddSingleton(repository)
            .BuildServiceProvider();
        using var cancellation = new CancellationTokenSource();

        A.CallTo(() => consumer.Listen<OrderMessageDto>(
                RabbitMqConstants.Queues.OrderCozinhaQueue,
                A<Func<OrderMessageDto, Task>>._,
                cancellation.Token))
            .ReturnsLazily(async call =>
            {
                var handler = call.GetArgument<Func<OrderMessageDto, Task>>(1);
                await handler(new OrderMessageDto { OrderId = orderId });
            });

        var worker = new TestableOrderWorker(provider, consumer, publisher);

        // act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => worker.RunAsync(cancellation.Token));

        // assert
        Assert.Contains($"Pedido {orderId} não encontrado", exception.Message);
        A.CallTo(() => publisher.PublishAsync(
                A<OrderStatusChangedMessageDto>._,
                A<string>._,
                A<string?>._))
            .MustNotHaveHappened();
    }

    private static Order CreateOrder(int orderId)
    {
        var customer = new Customer("Nome", "48999999999", "Rua", "Bairro", "123", "Cidade", "Referência", 88000000);
        var order = new Order(true, 10m, string.Empty, 1, DateTime.UtcNow);
        typeof(Order).GetProperty(nameof(Order.OrderId))!.SetValue(order, orderId);
        typeof(Order).GetProperty(nameof(Order.Customer))!.SetValue(order, customer);
        return order;
    }

    private sealed class TestableOrderWorker : OrderWorker
    {
        public TestableOrderWorker(IServiceProvider serviceProvider, IConsumer consumer, IMessagePublisher publisher)
            : base(serviceProvider, consumer, publisher)
        {
        }

        public Task RunAsync(CancellationToken cancellationToken) => ExecuteAsync(cancellationToken);
    }
}
