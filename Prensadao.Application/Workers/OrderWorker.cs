using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prensadao.Application.Consumers;
using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;
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

        _consumer.Listen<OrderMessageDTO>(RabbitMqConstants.Queues.OrderCozinhaQueue, async message =>
        {
            using var scope = _serviceProvider.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

            var order = await orderRepository.GetById(message.OrderId);

            if (order is null) 
            {
                Console.WriteLine($"Pedido {message.OrderId} não encontrado.");
                return;
            }

            order.AtualizaStatusPreparacao();
            await orderRepository.Update(order);

            Console.WriteLine($"Pedido #{order.OrderId} atualizado para Em Preparacao");
        });

        return Task.CompletedTask;
    }
}
