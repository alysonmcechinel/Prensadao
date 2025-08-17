using RabbitMQ.Client;

namespace Prensadao.Infra.Messaging.Interfaces;

public interface IRabbitMqConfig
{
    IModel CreateChannel();
    Task Config();
    void Dispose();
}
