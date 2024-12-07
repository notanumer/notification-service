using DatabaseService.DataAccess;
using DatabaseService.DataAccess.Abstractions;
using DatabaseService.DataAccess.RabbitMq;
using DatabaseService.Infrastructure;
using DatabaseService.Services;
using DatabaseService.Services.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var databaseConnectionString = builder.Configuration.GetConnectionString("PgConnection")
                               ?? throw new ArgumentNullException("ConnectionStrings:AppDatabase",
                                   "Database connection string is not initialized");

builder.Services
    .AddControllers()
    .AddMvcOptions(opt =>
    {
        opt.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    }).AddNewtonsoftJson();

builder.Services.AddDbContext<AppDbContext>(
    new DbContextOptionsSetup(databaseConnectionString).Setup);
builder.Services.AddAsyncInitializer<DatabaseInitializer>();
builder.Services.AddScoped<IAppDbContext>(s => s.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IRabbitMqService, RabbitMqService>();
builder.Services.AddTransient<IEventService, EventService>();



var app = builder.Build();

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