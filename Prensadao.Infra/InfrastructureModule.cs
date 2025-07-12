using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prensadao.Domain.Entities;
using Prensadao.Domain.Repositories;
using Prensadao.Infra.Persistence;
using Prensadao.Infra.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prensadao.Infra
{
    public static class InfrastructureModule
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddData(configuration)
                .AddRepositories();

            return services;
        }

        // Configuração do banco de dados
        public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        {
            //TODO: CONFIGURAR BASE DEPOIS
            services.AddDbContext<PrensadaoDbContext>(o => o.UseInMemoryDatabase("dbPrensadao"));

            return services;
        }

        // Injeção de dependencia dos repositorios
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IOrderRepository, OrderRepository>();

            return services;
        }

        public static void Seed(PrensadaoDbContext dbContext)
        {
            var customer = new Customer("Teste banco", 123456789, "rua joao", "Centro", "S/N", "Criciuma", "Predio bonito", 88850000);

            dbContext.Add(customer);
            dbContext.SaveChanges();
        }
    }
}
