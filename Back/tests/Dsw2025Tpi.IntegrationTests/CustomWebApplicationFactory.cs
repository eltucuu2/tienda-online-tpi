using Dsw2025Tpi.Api;
using Dsw2025Tpi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dsw2025Tpi.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // La conexión queda abierta mientras viva la factory; si se cierra, SQLite borra la base en memoria
    private readonly SqliteConnection _connection = new("DataSource=Dsw2025TpiTests;Mode=Memory;Cache=Shared");

    public CustomWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // UseSetting llega antes de que Program lea la configuración (la clave HS256 debe tener 32+ caracteres)
        builder.UseSetting("Jwt:Key", "clave-de-pruebas-de-integracion-dsw2025-tpi-1234567890");

        builder.ConfigureTestServices(services =>
        {
            // Quitar los registros de SQL Server de ambos contextos
            // (en EF Core 9 la configuración de UseSqlServer vive en IDbContextOptionsConfiguration<T>)
            services.RemoveAll<DbContextOptions<AuthenticateContext>>();
            services.RemoveAll<DbContextOptions<Dsw2025TpiContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AuthenticateContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<Dsw2025TpiContext>>();

            // Ambos contextos comparten la misma base SQLite en memoria
            services.AddDbContext<AuthenticateContext>(options => options.UseSqlite(_connection));
            services.AddDbContext<Dsw2025TpiContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}