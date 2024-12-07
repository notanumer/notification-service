using DatabaseService.Models.Rabbit;

namespace DatabaseService.Services.Abstractions;

public interface IEventService
{
    Task SendEventAsync(Event notificationEvent, CancellationToken cancellationToken = default);
}