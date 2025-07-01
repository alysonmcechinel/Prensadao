using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Prensadao.Application.Publish;

public class Bus : IBus
{
    private readonly IModel _channel;

    public Bus()
    {
        var connectionFactory = new ConnectionFactory
        {
            HostName = "localhost"
        };

        var connection = connectionFactory.CreateConnection("prensadao-publisher");

        _channel = connection.CreateModel();
    }

    public Task Publish<T>(T message)
    {
        var json = JsonSerializer.Serialize(message);
        var byteArray = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish("order.created", "", null, byteArray);

        return Task.CompletedTask;
    }
}
