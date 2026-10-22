using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prensadao.Application;
using Prensadao.Application.DTOs;
using Prensadao.Application.Interfaces;

namespace Prensadao.Infra.Messaging.Workers;

public class OrderStatusMessageWorker : BackgroundService
{
    private readonly IConsumer _consumer;
    private readonly IOrderStatusMessageStrategyResolver _messageStrategyResolver;
    private readonly ILogger<OrderStatusMessageWorker> _logger;

    public OrderStatusMessageWorker(
        IConsumer consumer,
        IOrderStatusMessageStrategyResolver messageStrategyResolver,
        ILogger<OrderStatusMessageWorker> logger)
    {
        _consumer = consumer;
        _messageStrategyResolver = messageStrategyResolver;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.Listen<OrderStatusChangedMessageDto>(
            RabbitMqConstants.Queues.OrderStatusMessageQueue,
            message =>
            {
                if (!_messageStrategyResolver.TryResolve(message.Status, out var strategy))
                {
                    if (Enum.IsDefined(message.Status))
                    {
                        _logger.LogInformation(
                            "O status {OrderStatus} do pedido {OrderId} não produz mensagem.",
                            message.Status,
                            message.OrderId);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Valor de status inválido {OrderStatusValue} recebido para o pedido {OrderId}.",
                            (int)message.Status,
                            message.OrderId);

                        throw new InvalidOperationException(
                            $"Valor de status inválido {(int)message.Status} recebido para o pedido {message.OrderId}.");
                    }

                    return Task.CompletedTask;
                }

                _logger.LogInformation(
                    "Mensagem de status do pedido gerada: {OrderStatusMessage}",
                    strategy.BuildMessage(message));

                return Task.CompletedTask;
            },
            stoppingToken);

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}
