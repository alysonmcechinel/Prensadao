using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Prensadao.Application.Publish;

public class Bus : IBus
{
    private readonly IModel _channel;

    public Bus(IModel channel)
    {
        _channel = channel;
    }

    public Task Publish<T>(T message, string? routingKey = "")
    {
        var json = JsonSerializer.Serialize(message);
        var byteArray = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(RabbitMqConstants.Exchanges.OrderExchange, routingKey, null, byteArray);

        return Task.CompletedTask;
    }
}
