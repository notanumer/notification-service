using BaseMicroservice;
using DatabaseService.DataAccess;
using DatabaseService.DataAccess.Abstractions;
using DatabaseService.DataAccess.RabbitMq;
using DatabaseService.Infrastructure;
using DatabaseService.Services;
using DatabaseService.Services.Abstractions;

var builder = WebApplication.CreateBuilder(args);

var elasticUri = new Uri(builder.Configuration.GetConnectionString("ElasticSearchUri")
                         ?? throw new ArgumentNullException("ElasticSearchUri"));

builder.Services.AddElkLogging(elasticUri);

builder.Services.AddApplicationMetrics(b => b.AddMeter("notifications"));

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

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.UseTelemetryEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

await app.InitAsync();
await app.RunAsync();