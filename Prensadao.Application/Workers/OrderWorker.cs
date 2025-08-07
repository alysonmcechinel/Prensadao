using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prensadao.Application.Consumers;
using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
using Prensadao.Application.Publish;
using Prensadao.Domain.Enum;
using Prensadao.Domain.Repositories;

namespace Prensadao.Application.Workers;

public class OrderWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer _consumer;

    public OrderWorker(IServiceProvider serviceProvider, IConsumer consumer)
    {
        _serviceProvider = serviceProvider;
        _consumer = consumer;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // WORKER QUE RECEBE E ATUALIZA PEDIDOS ENVIADOS PARA COZINHA.
        _consumer.Listen<OrderMessageDTO>(RabbitMqConstants.Queues.OrderCozinhaQueue, async message =>
        {
            using var scope = _serviceProvider.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var bus = scope.ServiceProvider.GetRequiredService<IBus>();

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

                Console.WriteLine($"Pedido #{order.OrderId} atualizado para {order.OrderStatus}");

                var notify = new NotifyMessageDTO
                {
                    OrderId = order.OrderId,
                    ConsumerName = order.Customer.Name,
                    Delivery = order.Delivery,
                    OrderStatus = order.OrderStatus,
                    Phone = order.Customer.Phone
                };

                await bus.Publish(notify, RabbitMqConstants.Exchanges.NotifyExchange, "");
            }
            
        });

        return Task.CompletedTask;
    }
}
