using DatabaseService.DataAccess;
using DatabaseService.DataAccess.Abstractions;
using DatabaseService.DataAccess.RabbitMq;
using DatabaseService.Infrastructure;
using DatabaseService.Middlewares;
using DatabaseService.Services;
using DatabaseService.Services.Abstractions;
using Elastic.Serilog.Sinks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Enrichers.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

var elasticUri = new Uri(builder.Configuration.GetConnectionString("ElasticSearchUri")
                         ?? throw new ArgumentNullException("ElasticSearchUri"));

builder.Host.UseSerilog((context, config) =>
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

builder.Services.AddOpenTelemetry().WithMetrics(metrics =>
    {
        metrics
            .AddMeter("notifications")
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var databaseConnectionString = builder.Configuration.GetConnectionString("PgConnection")
                               ?? throw new ArgumentNullException("ConnectionStrings:AppDatabase",
                                   "Database connection string is not initialized");

builder.Services
    .AddControllers()
    .AddMvcOptions(opt => { opt.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true; })
    .AddNewtonsoftJson();

builder.Services.AddDbContext<AppDbContext>(
    new DbContextOptionsSetup(databaseConnectionString).Setup);
builder.Services.AddAsyncInitializer<DatabaseInitializer>();
builder.Services.AddScoped<IAppDbContext>(s => s.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IRabbitMqService, RabbitMqService>();
builder.Services.AddTransient<IEventService, EventService>();
builder.Services.AddMetrics();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseMiddleware<TraceIdMiddleware>();

app.UseOpenTelemetryPrometheusScrapingEndpoint();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

await app.InitAsync();
await app.RunAsync();