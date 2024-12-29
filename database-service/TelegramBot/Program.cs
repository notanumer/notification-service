using BaseMicroservice;
using Microsoft.AspNetCore.Mvc;
using TelegramBot.Models;
using TelegramBot.Services;

var builder = WebApplication.CreateBuilder(args);

var elasticUri = new Uri(builder.Configuration.GetConnectionString("ElasticSearchUri")
                         ?? throw new ArgumentNullException("ElasticSearchUri"));

builder.Services.AddElkLogging(elasticUri);
builder.Services.AddApplicationMetrics();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var apiKey = builder.Configuration.GetConnectionString("TelegramApiToken")
             ?? throw new ArgumentNullException("ConnectionStrings:TelegramApiToken",
                 "Telegram api key not found");
builder.Services.AddScoped(provider => new TelegramBotNotifyService(
    apiKey
));

builder.Services.AddHostedService(_ => new TelegramBotService(apiKey));
var app = builder.Build();

app.UseTelemetryEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/bot/send", async (
    [FromServices] TelegramBotNotifyService service,
    [FromBody] Payload payload
) =>
{
    await service.SendMessage(payload.UserId, payload.Text);
});

app.UseHttpsRedirection();

app.Run();