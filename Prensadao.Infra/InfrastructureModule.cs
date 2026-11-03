using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NodaTime;
using Prensadao.Application.Interfaces;
using Prensadao.Domain.Interfaces;
using Prensadao.Domain.Repositories;
using Prensadao.Infra.llm;
using Prensadao.Infra.Messaging.Interfaces;
using Prensadao.Infra.Messaging.RabbitMq;
using Prensadao.Infra.Messaging.Workers;
using Prensadao.Infra.Options;
using Prensadao.Infra.Persistence;
using Prensadao.Infra.Persistence.Repositories;
using RabbitMQ.Client;
using RabbitMqConnectionFactory = RabbitMQ.Client.IConnectionFactory;

namespace Prensadao.Infra
{
    public static class InfrastructureModule
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddData(configuration)
                .AddRabbitMQ(configuration)
                .AddHangFire(configuration)
                .AddWorkers()
                .AddNodaTime()
                .AddRepositories()
                .AddAzureOpenAi(configuration);

            return services;
        }

        // Configuração do banco de dados
        public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {

            //services.AddDbContext<PrensadaoDbContext>(o => o.UseInMemoryDatabase("dbPrensadao"));
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<PrensadaoDbContext>(o => o.UseNpgsql(connectionString));

            return services;
        }

        // Configuração do rabbitMQ, injeção de dependecia services e inicialiação do RabbitMQ
        public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddOptions<RabbitMqOptions>()
                .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
                .Validate(options => !string.IsNullOrWhiteSpace(options.Host),
                    $"{RabbitMqOptions.SectionName}:Host deve ser informado.")
                .Validate(options => options.Port is >= 1 and <= 65535,
                    $"{RabbitMqOptions.SectionName}:Port deve estar entre 1 e 65535.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.User),
                    $"{RabbitMqOptions.SectionName}:User deve ser informado.")
                .Validate(options => !string.IsNullOrWhiteSpace(options.Password),
                    $"{RabbitMqOptions.SectionName}:Password deve ser informado.")
                .ValidateOnStart();

            services.AddSingleton<RabbitMqConnectionFactory>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                return new ConnectionFactory
                {
                    HostName = options.Host,
                    Port = options.Port,
                    UserName = options.User,
                    Password = options.Password,
                    DispatchConsumersAsync = true
                };
            });

            services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();
            services.AddSingleton<IConsumer, Consumer>();

            services.AddSingleton<IRabbitMqConfig, RabbitMqConfig>();
            services.AddHostedService<RabbitMqStartup>();

            return services;
        }

        // Injeção de dependencia dos workers
        public static IServiceCollection AddWorkers(this IServiceCollection services)
        {
            services.AddHostedService<OrderWorker>();
            services.AddHostedService<OrderStatusMessageWorker>();

            return services;
        }

        // Configuração do Hangfire
        public static IServiceCollection AddHangFire(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddHangfire(cfg => cfg.UsePostgreSqlStorage(connectionString));

            services.AddHangfireServer();

            return services;
        }

        // Injeção de dependencia dos repositorios
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ISalesAnalyticsRepository, SalesAnalyticsRepository>();

            return services;
        }

        public static IServiceCollection AddAzureOpenAi(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureOpenAiOptions>(configuration.GetSection(AzureOpenAiOptions.SectionName));
            services.AddScoped<IPromotionSuggestionService, AzureOpenAiPromotionService>();

            return services;
        }



        // Injeção de dependencia do NodaTime
        public static IServiceCollection AddNodaTime(this IServiceCollection services)
        {
            services.AddSingleton<IClock>(SystemClock.Instance);

            return services;
        }

        /* TODO: Remover no futuro por enquanto sem necessidade
        public static void Seed(PrensadaoDbContext dbContext)
        {
            var customer = new Customer("Teste legal", 123456789, "rua joao", "Centro", "S/N", "Criciuma", "Predio bonito", 88850000);

            dbContext.Add(customer);

            var product = new Product("X Normal", 10.50m , "O melhor X da cidade");
            var product2 = new Product("X Frango", 12.50m, "O melhor X da cidade");

            dbContext.AddRange(product, product2);

            dbContext.SaveChanges();
        }*/
    }
}
