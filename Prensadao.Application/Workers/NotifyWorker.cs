using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prensadao.Application.Consumers;
using Prensadao.Application.DTOs;
using Prensadao.Application.Helpers;
using Prensadao.Domain.Enum;
using Prensadao.Domain.Repositories;

namespace Prensadao.Application.Workers;

public class NotifyWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer _consumer;

    public NotifyWorker(IServiceProvider serviceProvider, IConsumer consumer)
    {
        _serviceProvider = serviceProvider;
        _consumer = consumer;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // WORKER QUE RECEBE NOTIFICA CLIENTE DO STATUS DO PEDIDO.
        _consumer.Listen<NotifyMessageDTO>(RabbitMqConstants.Queues.OrderNotifyQueue, async message =>
        {
            using var scope = _serviceProvider.CreateScope();
            //var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

            if (message.OrderStatus == OrderStatusEnum.EmPreparacao)
                Console.WriteLine($"O seu pedido N°{message.OrderId} está em {OrderStatusEnum.EmPreparacao.GetDescription()}!!");
            else if(message.OrderStatus == OrderStatusEnum.Pronto && !message.Delivery)
                Console.WriteLine($"O seu pedido N°{message.OrderId} está {OrderStatusEnum.EmPreparacao.GetDescription()}, pode vir buscalo!");
            else if (message.OrderStatus == OrderStatusEnum.SaiuParaEntrega)
                Console.WriteLine($"O seu pedido N°{message.OrderId} {OrderStatusEnum.EmPreparacao.GetDescription()}!!");
            else if (message.OrderStatus == OrderStatusEnum.Finalizado)
                Console.WriteLine($"O seu pedido N°{message.OrderId} foi concluido, agradeços a preferencia otimo apetite!!");
        });

        return Task.CompletedTask;
    }
}
