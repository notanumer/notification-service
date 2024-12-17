using System.Text;
using System.Text.Json;
using DatabaseService.Models.Rabbit;
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

    public async Task<bool> SendMessageAsync(Event e, CancellationToken cancellationToken = default)
    {
        string queue;
        switch (e.ChannelType)
        {
            case ChannelType.Telegram:
                queue = "Telegram";
                break;
            case ChannelType.VK:
                queue = "VK";
                break;
            default:
                throw new NotSupportedException("Channel type not supported");
        }
        var message = JsonSerializer.Serialize(e);
        return await SendMessageAsync(message, queue, cancellationToken);
    }

    public async Task<bool> SendMessageAsync(object obj, CancellationToken cancellationToken = default)
    {
        if (obj is Event ev)
            return await SendMessageAsync(ev, cancellationToken);
        var message = JsonSerializer.Serialize(obj);
        return await SendMessageAsync(message, "Event", cancellationToken);
    }

    public async Task<bool> SendMessageAsync(string message, string queue, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _factory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(null, cancellationToken);
            await channel.QueueDeclareAsync(queue: queue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            var body = Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync("", queue, false, body, cancellationToken);
            return true;
        }
        catch (Exception exception)
        {
            return false;
        }
    }
}