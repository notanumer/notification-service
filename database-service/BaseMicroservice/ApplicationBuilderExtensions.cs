using BaseMicroservice.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace BaseMicroservice;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseTelemetryEndpoints(this IApplicationBuilder app)
    {
        app.UseMiddleware<TraceIdMiddleware>();
        app.UseOpenTelemetryPrometheusScrapingEndpoint();
        
        return app;
    }
}