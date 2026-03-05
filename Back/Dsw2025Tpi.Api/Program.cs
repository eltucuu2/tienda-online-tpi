using Dsw2025Tpi.Api.Helpers;
using Dsw2025Tpi.Application.Helpers;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Helpers;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure; // <--- AGREGADO (Vital para GetService)
using Microsoft.EntityFrameworkCore.Storage;        // <--- AGREGADO (Vital para CreateTables)
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog.Extensions.Logging;
using System.Text;

namespace Dsw2025Tpi.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddLoggingService(builder.Configuration);
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerDocumentation();
        builder.Services.AddHealthChecks();
        builder.Services.AddAuthenticationAndAuthorization(builder.Configuration);
        builder.Services.AddDbContexts(builder.Configuration);
        builder.Services.AddAplicationServices();

        // Configuración CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowReactFrontend",
                policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
        });

        var app = builder.Build();

        // --- SEEDING DE BASE DE DATOS ---
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<Dsw2025TpiContext>();
                var authContext = services.GetRequiredService<AuthenticateContext>();

               
                
                // 2. CREAR TABLAS DE IDENTIDAD (Usuarios)
                authContext.Database.EnsureCreated();

                // 3. CREAR TABLAS DE NEGOCIO (Productos/Ordenes) MANUALMENTE
                // Usamos GetService para forzar la creación de las tablas que faltan
                var databaseCreator = context.Database.GetService<IRelationalDatabaseCreator>();
                try 
                {
                    databaseCreator.CreateTables();
                }
                catch (Exception) 
                {
                    // Si ya existen, no pasa nada
                }

                // 4. CARGAR DATOS
                context.Seedwork<Customer>("Sources\\customers.json");
                context.Seedwork<Product>("Sources\\products.json");
                context.Seedwork<Order>("Sources\\orders.json");
                context.Seedwork<OrderItem>("Sources\\orderitems.json");
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Ocurrió un error durante el seeding de la base de datos.");
            }
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowReactFrontend");
        app.UseCustomMiddlewares();
        app.MapControllers();
        app.MapHealthChecks("/healthcheck");

        app.Run();
    }
}