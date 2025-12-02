using Microsoft.Extensions.Hosting;
using Prensadao.Application;
using Prensadao.Application.DTOs;
using Prensadao.Application.Helpers;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Enums;

namespace Prensadao.Infra.Messaging.Workers;

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
            switch (message.OrderStatus)
            {
                case OrderStatusEnum.EmPreparacao:
                    Console.WriteLine($"O seu pedido N°{message.OrderId} está em {OrderStatusEnum.EmPreparacao.GetDescription()}!!");
                    break;
                case OrderStatusEnum.Pronto:
                    if (message.Delivery)
                        Console.WriteLine($"O seu pedido N°{message.OrderId} está {OrderStatusEnum.Pronto.GetDescription()}, pode vir buscalo!");
                    break;
                case OrderStatusEnum.SaiuParaEntrega:
                    Console.WriteLine($"O seu pedido N°{message.OrderId} {OrderStatusEnum.SaiuParaEntrega.GetDescription()}!!");
                    break;
                case OrderStatusEnum.Cancelado:
                    Console.WriteLine($"O seu pedido N°{message.OrderId} foi {OrderStatusEnum.Cancelado.GetDescription()} :(");
                    break;
                case OrderStatusEnum.Finalizado:
                    Console.WriteLine($"O seu pedido N°{message.OrderId} foi concluido, agradeços a preferencia otimo apetite!!");
                    break;
                default:
                    Console.WriteLine($"O seu pedido N°{message.OrderId} com status, não identificado.");
                    break;
            }
        });

        return Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
