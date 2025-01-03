using BaseMicroservice;
using TelegramNotifier.Consumers;
using TelegramNotifier.Services;

var builder = WebApplication.CreateBuilder(args);

var elasticUri = new Uri(builder.Configuration.GetConnectionString("ElasticSearchUri")
                         ?? throw new ArgumentNullException("ElasticSearchUri"));

builder.Services.AddElkLogging(elasticUri);
builder.Services.AddApplicationMetrics();

var userApiUri = builder.Configuration.GetConnectionString("UserApiUri")
                 ?? throw new ArgumentNullException("UserApiUri",
                     "User api uri uri is not initialized");

builder.Services.AddSingleton(provider => new UserCredentialsService(userApiUri));

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

builder.Services.AddSingleton<IUserCredentialsService>(provider => new UserCredentialsService(userApiUri));

builder.Services.AddSingleton<BaseRabbitConsumer>(provider => new TelegramMessagesConsumer(
    provider.GetService<IUserCredentialsService>(),
    botUri,
    rabbitUri,
    queueName,
    provider.GetService<ILogger<TelegramMessagesConsumer>>()
));

builder.Services.AddHostedService<MainService>();

var app = builder.Build();

app.UseTelemetryEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

/*app.MapPatch("/register/{userName}/{chatId}", async (
    [FromRoute] string userName,
    [FromRoute] string chatId,
    [FromServices] IUserService service
) =>
{
    await service.PatchCredentials(
        userName,
        new DatabaseService.Models.Postgres.Credentials() { TelegramChatId = chatId }
    );
});*/

app.Run();