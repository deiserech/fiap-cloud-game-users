using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace FiapCloudGames.Users.Api.Extensions
{
    public static class OpenTelemetryServiceCollectionExtensions
    {
        public static IServiceCollection AddFiapCloudGamesOpenTelemetry(this IServiceCollection services)
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService("FiapCloudGamesWebApp"))
                .WithTracing(builder =>
                {
                    builder
                        .AddSource("FiapCloudGames.Application")
                        .AddAspNetCoreInstrumentation()
                        .AddSqlClientInstrumentation()
                        .AddOtlpExporter(ConfigureOtlpExporter);
                })
                .WithLogging(builder =>
                {
                    builder.AddOtlpExporter(ConfigureOtlpExporter);
                })
                .WithMetrics(builder =>
                {
                    builder
                        .AddAspNetCoreInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddOtlpExporter(ConfigureOtlpExporter);
                });

            return services;
        }

        private static void ConfigureOtlpExporter(OtlpExporterOptions options)
        {
            options.Endpoint = new Uri("https://otlp.nr-data.net:4317");
            var newRelicKey = Environment.GetEnvironmentVariable("NEW_RELIC_LICENSE_KEY");
            options.Headers = $"api-key={newRelicKey}";
        }
    }
}
