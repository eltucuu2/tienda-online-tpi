namespace Dsw2025Tpi.Api.Helpers
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomMiddlewares(this IApplicationBuilder app)
        {
            app.UseCors("PermitirFrontend");
            app.UseExceptionMiddleware();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }
    }
}
