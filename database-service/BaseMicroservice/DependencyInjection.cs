using Elastic.Serilog.Sinks;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.OpenTelemetry;

namespace BaseMicroservice;

public static class DependencyInjection
{
    public static IServiceCollection AddElkLogging(this IServiceCollection services, Uri elasticUri)
    {
        services.AddSerilog(config =>
        {
            config
                .Enrich.WithOpenTelemetryTraceId()
                .Enrich.FromLogContext()
                .WriteTo.Logger(c =>
                {
                    c.Filter.ByExcluding(e => e.Properties.TryGetValue("Path", out var path) &&
                                              path.ToString().Trim('"') == "/metrics");
                    c.WriteTo.Console();
                })
                .WriteTo.Logger(c => c.WriteTo.Elasticsearch(new List<Uri> { elasticUri }));
        });

        return services;
    }

    public static IServiceCollection AddApplicationMetrics(this IServiceCollection services,
        Action<MeterProviderBuilder>? setup = null)
    {
        services.AddMetrics();
        services.AddOpenTelemetry().WithMetrics(metrics =>
            {
                setup?.Invoke(metrics);
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();
            })
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        return services;
    }
}