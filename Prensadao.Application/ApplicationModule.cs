using Microsoft.Extensions.DependencyInjection;
using Prensadao.Application.Interfaces;
using Prensadao.Application.Services;

namespace Prensadao.Application
{
    public static class ApplicationModule
    {
        public static IServiceCollection AddAplications(this IServiceCollection services)
        {
            services
                .AddServices();

            return services;
        }

        // Injeção de dependencia das services
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderItemService, OrderItemService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IProductService, ProductService>();

            return services;
        }
    }
}
