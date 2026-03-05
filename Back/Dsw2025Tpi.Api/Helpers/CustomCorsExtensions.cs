namespace Dsw2025Tpi.Api.Helpers
{
    public static class CustomCorsExtensions
    {
        public static IServiceCollection AddCustomCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("PermitirFrontend", policy =>
                    policy.AllowAnyOrigin() // <--- Usa esto solo para probar
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });

            return services;
        }
    }
}
