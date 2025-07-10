using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prensadao.Application.Services
{
    public interface IRabbitMqConfigService
    {
        IModel CreateChannel();
        Task Config();
    }
}
