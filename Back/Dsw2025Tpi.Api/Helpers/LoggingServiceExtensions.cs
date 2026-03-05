namespace Dsw2025Tpi.Api.Helpers
{
    public static class LoggingServiceExtensions
    {
        public static IServiceCollection AddLoggingService(this IServiceCollection services, IConfiguration config)
        {
            var path = config.GetSection("LogPath").Value;

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();

                loggingBuilder.AddConsole();

                loggingBuilder.AddSimpleConsole(formatter =>
                {
                    formatter.TimestampFormat = "[dd-MM-yyyy]-[HH:mm:ss.fff] ";
                    formatter.UseUtcTimestamp = true;
                    formatter.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled;
                    formatter.IncludeScopes = true;
                    formatter.SingleLine = true;
                });

                loggingBuilder.AddFile(
                    path,
                    outputTemplate: "{Timestamp:[dd-MM-yyyy]-[HH:mm:ss.fff]} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
                    minimumLevel: LogLevel.Debug,
                    levelOverrides: new Dictionary<string, LogLevel>
                    {
                        ["Microsoft"] = LogLevel.Warning,
                        ["Microsoft.AspNetCore"] = LogLevel.Warning,
                        ["Microsoft.EntityFrameworkCore"] = LogLevel.Warning
                    }
                );
            });

            return services;
        }
    }
}
