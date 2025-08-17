using Prensadao.Application.Interfaces;
using Prensadao.Infra.Messaging.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Prensadao.Infra.Messaging.RabbitMq;

public class Bus : IBus
{
    private readonly IRabbitMqConfig _rabbitMqConfigService;

    public Bus(IRabbitMqConfig rabbitMqConfigService)
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

        var props = channel.CreateBasicProperties();
        props.Persistent = true; // garante persistência ao publicar

        channel.BasicPublish(exchange, routingKey, props, byteArray);

        return Task.CompletedTask;
    }
}
