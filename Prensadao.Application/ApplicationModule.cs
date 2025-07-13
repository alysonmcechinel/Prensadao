using Microsoft.Extensions.DependencyInjection;
using Prensadao.Application.Interfaces;
using Prensadao.Application.Publish;
using Prensadao.Application.Services;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prensadao.Application
{
    public static class ApplicationModule
    {
        public static IServiceCollection AddAplications(this IServiceCollection services)
        {
            services
                .AddRabbitMQ()
                .AddServices();

            return services;
        }

        // Injeção de dependencia das services
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IBus, Bus>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderItemService, OrderItemService>();

            return services;
        }

        // Configuração do rabbitMQ, injeção de dependecia services e inicialiação do RabbitMQ
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services)
        {
            services.AddHostedService<RabbitMqStartupService>();
            services.AddSingleton<IRabbitMqConfigService, RabbitMqConfigService>();

            services.AddScoped<IModel>(x =>
            {
                var rabbitConfig = x.GetRequiredService<RabbitMqConfigService>();
                return rabbitConfig.CreateChannel();
            });

            return services;
        }
    }
}
