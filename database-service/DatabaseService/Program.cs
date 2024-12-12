using DatabaseService.DataAccess;
using DatabaseService.DataAccess.Abstractions;
using DatabaseService.DataAccess.RabbitMq;
using DatabaseService.Infrastructure;
using DatabaseService.Services;
using DatabaseService.Services.Abstractions;
using Elastic.Serilog.Sinks;
using Elastic.Transport;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var elasticUri = new Uri(builder.Configuration.GetConnectionString("ElasticSearchUri")
                         ?? throw new ArgumentNullException("ElasticSearchUri"));

builder.Host.UseSerilog((context, config) =>
{
    config
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(
            new ElasticsearchSinkOptions(
                new DistributedTransport(
                    new TransportConfiguration(elasticUri))));
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

app.MapPrometheusScrapingEndpoint("/metrics");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();
app.UseHttpsRedirection();

await app.InitAsync();
await app.RunAsync();