namespace DatabaseService.DataAccess.RabbitMq;

public interface IRabbitMqService
{
    Task<bool> SendMessageAsync(object obj, CancellationToken cancellationToken = default);
    Task<bool> SendMessageAsync(string message, CancellationToken cancellationToken = default);
}