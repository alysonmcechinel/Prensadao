using Microsoft.Extensions.Hosting;
using Prensadao.Application.Interfaces;

namespace Prensadao.Infra.Messaging.RabbitMq
{
    public class RabbitMqStartup : IHostedService
    {
        private readonly IRabbitMqConfig _rabbitMqConfig;

        public RabbitMqStartup(IRabbitMqConfig rabbitMqConfig)
        {
            _rabbitMqConfig = rabbitMqConfig;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _rabbitMqConfig.Config();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
