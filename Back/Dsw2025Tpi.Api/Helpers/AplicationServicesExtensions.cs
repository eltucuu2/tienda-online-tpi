using Dsw2025Tpi.Application.Helpers;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Interfaces;

namespace Dsw2025Tpi.Api.Helpers
{
    public static class AplicationServicesExtensions
    {
        public static IServiceCollection AddAplicationServices(this IServiceCollection services)
        {
            // Servicio que genera los tokens JWT (Singleton está bien porque no depende del contexto)
            services.AddSingleton<JwtTokenService>();
            
            services.AddScoped<IRepository, EfRepository>(); // Patrón repositorio (Scoped)

            // CAMBIO IMPORTANTE: Cambiar AddTransient por AddScoped
            services.AddScoped<IProductsManagementService, ProductsManagementService>();
            services.AddScoped<ProductsManagmentServiceExtensions>();
            
            services.AddScoped<IOrdersManagementService, OrdersManagementService>();
            services.AddScoped<OrdersManagementServiceExtensions>();

            return services;
        }
    }
}