using DatabaseService.DataAccess.Abstractions;
using DatabaseService.DataAccess.RabbitMq;
using DatabaseService.Models.Postgres;
using DatabaseService.Models.Rabbit;
using DatabaseService.Services.Abstractions;
using Newtonsoft.Json;

namespace DatabaseService.Services;

public class EventService : IEventService
{
    private readonly IAppDbContext _appDbContext;
    private readonly IRabbitMqService _rabbitMqService;

    public EventService(IRabbitMqService rabbitMqService, IAppDbContext appDbContext)
    {
        _rabbitMqService = rabbitMqService;
        _appDbContext = appDbContext;
    }

    public async Task SendEventAsync(Event notificationEvent, CancellationToken cancellationToken = default)
    {
        notificationEvent.NotificationId = Guid.NewGuid().ToString();
        var message = JsonConvert.SerializeObject(notificationEvent);
        var isSuccess = await _rabbitMqService.SendMessageAsync(message, cancellationToken);

        var notification = new Notification()
        {
            MessageContent = notificationEvent.MessageContent.Text,
            MessageType = notificationEvent.EventType,
            Recipient = notificationEvent.Recipient,
            IsSuccess = isSuccess,
            CreatedAt = DateTimeOffset.Now,

        };

        await _appDbContext.Notifications.AddAsync(notification, cancellationToken);
    }
}