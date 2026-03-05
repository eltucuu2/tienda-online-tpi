using Dsw2025Tpi.Api.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Dsw2025Tpi.Api.Helpers
{
    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
