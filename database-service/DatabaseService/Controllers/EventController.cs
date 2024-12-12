using System.Diagnostics.Metrics;
using DatabaseService.Models.Rabbit;
using DatabaseService.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseService.Controllers;

[ApiController]
[Route("event")]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly Counter<int> _notificationSent;

    public EventController(IEventService eventService, IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("notifications");
        _notificationSent = meter.CreateCounter<int>("notification_sent");
        _eventService = eventService;
    }

    [HttpPost("send")]
    public async Task SendEvent([FromBody] Event notificationEvent, CancellationToken cancellationToken = default)
    {
        _notificationSent.Add(1, new KeyValuePair<string, object?>("type", notificationEvent.ChannelType));
        await _eventService.SendEventAsync(notificationEvent, cancellationToken);
    }
}