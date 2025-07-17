using RabbitMQ.Client;

namespace Prensadao.Application.Services
{
    public interface IRabbitMqConfigService
    {
        IModel CreateChannel();
        Task Config();
    }
}
