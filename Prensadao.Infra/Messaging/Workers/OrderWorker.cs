using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prensadao.Application;
using Prensadao.Application.DTOs;
using Prensadao.Application.Helpers;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;
using Prensadao.Domain.Repositories;

namespace Prensadao.Infra.Messaging.Workers;

public class OrderWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer _consumer;
    private readonly IBus _bus;

    public OrderWorker(IServiceProvider serviceProvider, IConsumer consumer, IBus bus)
    {
        _serviceProvider = serviceProvider;
        _consumer = consumer;
        _bus = bus;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // WORKER QUE RECEBE E ATUALIZA PEDIDOS ENVIADOS PARA COZINHA.
        _consumer.Listen<OrderMessageDto>(RabbitMqConstants.Queues.OrderCozinhaQueue, async message =>
        {
            using var scope = _serviceProvider.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

            var order = await orderRepository.GetById(message.OrderId);

            if (order is null) 
            {
                Console.WriteLine($"Pedido {message.OrderId} não encontrado.");
                return;
            }

            if (order.OrderStatus == OrderStatusEnum.Criado)
            {
                order.NextStatus();
                await orderRepository.Update(order);

                Console.WriteLine($"Pedido #{order.OrderId} atualizado para {order.OrderStatus.GetDescription()}");

                var notify = new NotifyMessageDto
                {
                    OrderId = order.OrderId,
                    ConsumerName = order.Customer.Name,
                    Delivery = order.Delivery,
                    OrderStatus = order.OrderStatus,
                    Phone = order.Customer.Phone
                };

                await _bus.Publish(notify, RabbitMqConstants.Exchanges.NotifyExchange, "");
            }
            
        });

        return Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
