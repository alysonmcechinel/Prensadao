using Microsoft.Extensions.DependencyInjection;
using Prensadao.Application.Consumers;
using Prensadao.Application.Interfaces;
using Prensadao.Application.Publish;
using Prensadao.Application.Services;
using Prensadao.Application.Workers;
using RabbitMQ.Client;

namespace Prensadao.Application
{
    public static class ApplicationModule
    {
        public static IServiceCollection AddAplications(this IServiceCollection services)
        {
            services
                .AddRabbitMQ()
                .AddServices()
                .AddWorkers();

            return services;
        }

        // Injeção de dependencia das services
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IBus, Bus>();
            services.AddSingleton<IConsumer, Consumer>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderItemService, OrderItemService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IProductService, ProductService>();

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

        public static IServiceCollection AddWorkers(this IServiceCollection services) 
        {
            services.AddHostedService<OrderWorker>();

            return services;
        }
    }
}
