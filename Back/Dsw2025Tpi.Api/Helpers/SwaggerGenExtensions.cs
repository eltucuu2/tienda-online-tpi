using Microsoft.OpenApi.Models;

namespace Dsw2025Tpi.Api.Helpers
{
    public static class SwaggerGenExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(o =>
            {
                o.CustomSchemaIds(type => type.FullName.Replace("+", "."));
                o.SwaggerDoc("v1", new OpenApiInfo { Title = "Dsw2025Tpi", Version = "v1" });
                //Agrega soporte para seguridad utilizando el esquema JWT Bearer
                o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Description = "Ingrese el token JWT",
                    Type = SecuritySchemeType.ApiKey //El tipo es ApiKey (así es como Swagger representa el uso de headers con tokens).
                });
                //Esta parte enlaza la definición "Bearer" como requisito global de seguridad.
                o.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

            return services;
        }
    }
}
