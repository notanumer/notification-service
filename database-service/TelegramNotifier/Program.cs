using BaseMicroservice;
using TelegramNotifier.Services;

var builder = WebApplication.CreateBuilder(args);

var elasticUri = new Uri(builder.Configuration.GetConnectionString("ElasticSearchUri")
                         ?? throw new ArgumentNullException("ElasticSearchUri"));

builder.Services.AddElkLogging(elasticUri);
builder.Services.AddApplicationMetrics();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var rabbitUri = builder.Configuration.GetConnectionString("RabbitMqUri")
                ?? throw new ArgumentNullException("RabbitMqUri",
                    "Rabbit connection string is not initialized");

var queueName = builder.Configuration.GetConnectionString("QueueName")
                ?? throw new ArgumentNullException("QueueName",
                    "Rabbit connection string is not initialized");
var botUri = builder.Configuration.GetConnectionString("BotUri")
             ?? throw new ArgumentNullException("QueueName",
                 "Bot uri is not initialized");

builder.Services.AddHostedService<MainService>(provider => new MainService(
    botUri,
    rabbitUri,
    queueName
));

var app = builder.Build();

app.UseTelemetryEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.Run();