using Microsoft.Extensions.Hosting;
using Prensadao.Application;
using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;

namespace Prensadao.Infra.Messaging.Workers;

public class NotifyWorker : BackgroundService
{
    private readonly IConsumer _consumer;
    private readonly IOrderStatusNotificationStrategyFactory _notificationStrategyFactory;

    public NotifyWorker(IConsumer consumer, IOrderStatusNotificationStrategyFactory notificationStrategyFactory)
    {
        _consumer = consumer;
        _notificationStrategyFactory = notificationStrategyFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.Listen<NotifyMessageDto>(RabbitMqConstants.Queues.OrderNotifyQueue, message =>
        {
            var strategy = _notificationStrategyFactory.GetStrategy(message.OrderStatus);
            Console.WriteLine(strategy.BuildMessage(message));
            return Task.CompletedTask;
        }, stoppingToken);

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}
