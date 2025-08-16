using Prensadao.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Prensadao.Infra.Messaging.RabbitMq;

public class Consumer : IConsumer
{
    private readonly IRabbitMqConfig _rabbitMqConfigService;

    public Consumer(IRabbitMqConfig rabbitMqConfigService)
    {
        _rabbitMqConfigService = rabbitMqConfigService;
    }

    public async Task Listen<T>(string queue, Func<T, Task> onMessage)
    {
        if (string.IsNullOrEmpty(queue))
            throw new ArgumentNullException(nameof(queue));

        var channel = _rabbitMqConfigService.CreateChannel();


        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var content = Encoding.UTF8.GetString(body);

            try
            {
                var message = JsonSerializer.Deserialize<T>(content);

                if (message is not null)
                {
                    await onMessage(message);
                    channel.BasicAck(eventArgs.DeliveryTag, false);
                }
                else
                {
                    Console.WriteLine("Mensagem nula recebida.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar a mensagem: {ex.Message}");
            }            
        };

        channel.BasicConsume(queue, autoAck: false, consumer);
    }
}
