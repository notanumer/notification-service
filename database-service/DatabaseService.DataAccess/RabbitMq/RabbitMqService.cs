using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace DatabaseService.DataAccess.RabbitMq;

public class RabbitMqService : IRabbitMqService
{
    private readonly ConnectionFactory _factory;

    public RabbitMqService(IConfiguration configuration)
    {
        var uri = configuration["ConnectionStrings:RabbitMqUri"]
                  ?? throw new ArgumentNullException();
        _factory = new ConnectionFactory
        {
            Uri = new Uri(uri)
        };
    }
    
    public async Task<bool> SendMessageAsync(object obj, CancellationToken cancellationToken = default)
    {
        var message = JsonSerializer.Serialize(obj);
        return await SendMessageAsync(message, cancellationToken);
    }

    public async Task<bool> SendMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(null, cancellationToken);
            await channel.QueueDeclareAsync(queue: "Events",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var body = Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync("", "Events", false, body, cancellationToken);
            return true;
        }
        catch (Exception exception)
        {
            return false;
        }
    }
}