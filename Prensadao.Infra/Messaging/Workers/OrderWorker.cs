using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prensadao.Application;
using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;
using Prensadao.Domain.Repositories;

namespace Prensadao.Infra.Messaging.Workers;

public class OrderWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer _consumer;
    private readonly IMessagePublisher _messagePublisher;

    public OrderWorker(IServiceProvider serviceProvider, IConsumer consumer, IMessagePublisher messagePublisher)
    {
        _serviceProvider = serviceProvider;
        _consumer = consumer;
        _messagePublisher = messagePublisher;
    }

    // Mantém idempotência caso o RabbitMQ entregue a mesma mensagem mais de uma vez.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.Listen<OrderMessageDto>(RabbitMqConstants.Queues.OrderCozinhaQueue, async message =>
        {
            using var scope = _serviceProvider.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var order = await orderRepository.GetByIdAsync(message.OrderId);

            if (order is null || order.Status != OrderStatusEnum.Criado)
                return;

            order.AdvanceStatus();
            await orderRepository.UpdateAsync(order);

            var notify = new NotifyMessageDto
            {
                OrderId = order.OrderId,
                ConsumerName = order.Customer.Name,
                Delivery = order.IsDelivery,
                OrderStatus = order.Status,
                Phone = order.Customer.Phone
            };
            await _messagePublisher.PublishAsync(notify, RabbitMqConstants.Exchanges.NotifyExchange, "");

            Console.WriteLine($"Pedido #{order.OrderId} atualizado com sucesso.");
        }, stoppingToken);

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}
