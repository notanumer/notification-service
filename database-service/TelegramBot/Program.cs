using Microsoft.AspNetCore.Mvc;
using TelegramBot.Models;
using TelegramBot.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var apiKey = builder.Configuration.GetConnectionString("TelegramApiToken")
                               ?? throw new ArgumentNullException("ConnectionStrings:TelegramApiToken",
                                   "Telegram api kei not found");
builder.Services.AddScoped(provider => new TelegramBotNotifyService(apiKey));

builder.Services.AddHostedService(provider => new TelegramBotService(apiKey));
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use((req) =>
{
    Console.WriteLine(req.Method);
    return req;
});

app.MapPost("/bot/send", async (
        [FromServices] TelegramBotNotifyService service,
        [FromBody] Payload payload
    ) =>
{
    await service.SendMessage(payload.UserId, payload.Text);
});

app.UseHttpsRedirection();

app.Run();
