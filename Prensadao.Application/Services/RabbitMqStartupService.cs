using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prensadao.Application.Services
{
    public class RabbitMqStartupService : IHostedService
    {
        private readonly IRabbitMqConfigService _rabbitMqConfig;

        public RabbitMqStartupService(IRabbitMqConfigService rabbitMqConfig)
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
