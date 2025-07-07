using Prensadao.Application.Services;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Prensadao.Application.Publish;

public class Bus : IBus
{
    private readonly IRabbitMqConfigService _rabbitMqConfigService;

    public Bus(IRabbitMqConfigService rabbitMqConfigService)
    {
        _rabbitMqConfigService = rabbitMqConfigService;
    }

    public Task Publish<T>(T message, string exchange, string? routingKey = "")
    {
        if (string.IsNullOrWhiteSpace(exchange))
            throw new ArgumentNullException(nameof(exchange));

        if (message is null) 
            throw new ArgumentNullException(nameof(message));

        var json = JsonSerializer.Serialize(message);
        var byteArray = Encoding.UTF8.GetBytes(json);

        using var channel = _rabbitMqConfigService.CreateChannel();

        channel.BasicPublish(exchange, routingKey, null, byteArray);

        return Task.CompletedTask;
    }
}
