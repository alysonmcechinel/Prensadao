using RabbitMQ.Client;

namespace Prensadao.Application.Interfaces
{
    public interface IRabbitMqConfig
    {
        IModel CreateChannel();
        Task Config();
    }
}
