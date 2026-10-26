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

            // Reintentos ante fallas transitorias: Azure SQL serverless tarda en reactivarse si estaba pausada
            services.AddDbContext<AuthenticateContext>(options =>
                options.UseSqlServer(connection, sql => sql.EnableRetryOnFailure(
                    maxRetryCount: 6, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null)));

            services.AddDbContext<Dsw2025TpiContext>(options =>
                options.UseSqlServer(connection, sql => sql.EnableRetryOnFailure(
                    maxRetryCount: 6, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null)));

            return services;
        }
    }
}

