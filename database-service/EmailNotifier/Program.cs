using BaseMicroservice;
using EmailNotifier.Configuration;
using EmailNotifier.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var elasticUri = new Uri(builder.Configuration.GetConnectionString("ElasticSearchUri")
                         ?? throw new ArgumentNullException("ElasticSearchUri"));

builder.Services.AddElkLogging(elasticUri);
builder.Services.AddApplicationMetrics();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var userApiUri = builder.Configuration.GetConnectionString("UserApiUri")
                 ?? throw new ArgumentNullException("UserApiUri",
                     "User api uri is not initialized");

var rabbitUri = builder.Configuration.GetConnectionString("RabbitMqUri")
                ?? throw new ArgumentNullException("RabbitMqUri",
                    "Rabbit connection string is not initialized");

var queueName = builder.Configuration.GetConnectionString("RabbitMqQueueName")
                ?? throw new ArgumentNullException("RabbitMqQueueName",
                    "Rabbit connection string is not initialized");

builder.Services.AddSingleton(provider => new UserCredentialsService(userApiUri));

builder.Services.Configure<MessageSettings>(
    builder.Configuration.GetSection("MessageSettings"));

builder.Services.AddHostedService<MainService>(provider => new MainService(
    provider.GetService<UserCredentialsService>(),
    provider.GetRequiredService<IOptions<MessageSettings>>().Value,
    rabbitUri,
    queueName,
    provider
));

var app = builder.Build();

app.UseTelemetryEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.Run();