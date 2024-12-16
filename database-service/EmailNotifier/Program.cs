using EmailNotifier.Configuration;
using EmailService.Services;
using Microsoft.Extensions.Options;

namespace EmailNotifier;

class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var rabbitUri = builder.Configuration.GetConnectionString("RabbitMqUri")
                        ?? throw new ArgumentNullException("RabbitMqUri",
                            "Rabbit connection string is not initialized");

        var queueName = builder.Configuration.GetConnectionString("RabbitMqQueueName")
                        ?? throw new ArgumentNullException("RabbitMqQueueName",
                            "Rabbit connection string is not initialized");

        builder.Services.Configure<MessageSettings>(
            builder.Configuration.GetSection("MessageSettings"));

        builder.Services.AddHostedService<MainService>(provider => new MainService(
            provider.GetRequiredService<IOptions<MessageSettings>>().Value,
            rabbitUri,
            queueName
        ));

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();


        app.Run();
    }
}