using Prensadao.Application.Interfaces;
using Prensadao.Infra.Messaging.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Prensadao.Infra.Messaging.RabbitMq;

public class Consumer : IConsumer
{
    private readonly IRabbitMqConfig _rabbitMqConfigService;
    private readonly ILogger<Consumer> _logger;

    public Consumer(IRabbitMqConfig rabbitMqConfigService, ILogger<Consumer> logger)
    {
        _rabbitMqConfigService = rabbitMqConfigService;
        _logger = logger;
    }

    public Task Listen<T>(string queue, Func<T, Task> onMessage, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(queue))
            throw new ArgumentNullException(nameof(queue));

        ArgumentNullException.ThrowIfNull(onMessage);

        var channel = _rabbitMqConfigService.CreateChannel();

        // Limita entregas não confirmadas por consumidor
        channel.BasicQos(0, prefetchCount: 10, global: false);

        // async-safe
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.Received += async (model, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var content = Encoding.UTF8.GetString(body);

            try
            {
                var message = JsonSerializer.Deserialize<T>(content)
                    ?? throw new JsonException($"A mensagem da fila {queue} foi desserializada como nula.");

                await onMessage(message);
                channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falha ao processar mensagem da fila {Queue}. A mensagem será encaminhada para a dead-letter queue.",
                    queue);

                if (channel.IsOpen)
                    channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
        };

        var consumerTag = channel.BasicConsume(queue, autoAck: false, consumer);

        cancellationToken.Register(() =>
        {
            try
            {
                if (!channel.IsOpen)
                    return;

                channel.BasicCancel(consumerTag);
                channel.Close();
                channel.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao encerrar o consumidor da fila {Queue}.", queue);
            }
        });

        return Task.CompletedTask;
    }
}
