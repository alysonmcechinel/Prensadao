using RabbitMQ.Client;

namespace Prensadao.Infra.Messaging.Interfaces;

public interface IRabbitMqConfig : IDisposable
{
    IModel CreateChannel();
    Task Config();
}
