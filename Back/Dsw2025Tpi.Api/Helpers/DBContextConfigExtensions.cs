using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Helpers;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Api.Helpers
{
    public static class DBContextConfigExtensions
    {
        public static IServiceCollection AddDbContexts(this IServiceCollection services, IConfiguration config)
        {
            var connection = config.GetConnectionString("Dsw2025TpiEntities");

            services.AddDbContext<AuthenticateContext>(options =>
                options.UseSqlServer(connection));

                services.AddDbContext<Dsw2025TpiContext>(options =>
                options.UseSqlServer(connection));

            return services;
        }
    }
}

