using Microsoft.Extensions.Hosting;
using Prensadao.Application.DTOs;
using Prensadao.Application.Helpers;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;

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
        _consumer.Listen<NotifyMessageDto>(RabbitMqConstants.Queues.OrderNotifyQueue, async message =>
        {
            if (message.OrderStatus == OrderStatusEnum.EmPreparacao)
                Console.WriteLine($"O seu pedido N°{message.OrderId} está em {OrderStatusEnum.EmPreparacao.GetDescription()}!!");
            else if(message.OrderStatus == OrderStatusEnum.Pronto && !message.Delivery)
                Console.WriteLine($"O seu pedido N°{message.OrderId} está {OrderStatusEnum.Pronto.GetDescription()}, pode vir buscalo!");
            else if (message.OrderStatus == OrderStatusEnum.SaiuParaEntrega)
                Console.WriteLine($"O seu pedido N°{message.OrderId} {OrderStatusEnum.SaiuParaEntrega.GetDescription()}!!");
            else if(message.OrderStatus == OrderStatusEnum.Cancelado)
                Console.WriteLine($"O seu pedido N°{message.OrderId} foi {OrderStatusEnum.Cancelado.GetDescription()} :(");
            else if (message.OrderStatus == OrderStatusEnum.Finalizado)
                Console.WriteLine($"O seu pedido N°{message.OrderId} foi concluido, agradeços a preferencia otimo apetite!!");
        });

        return Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
