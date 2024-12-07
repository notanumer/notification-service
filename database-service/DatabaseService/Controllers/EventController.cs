using DatabaseService.Models.Rabbit;
using DatabaseService.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseService.Controllers;

[ApiController]
[Route("event")]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpPost("send")]
    public async Task SendEvent([FromBody] Event notificationEvent, CancellationToken cancellationToken = default)
        => await _eventService.SendEventAsync(notificationEvent, cancellationToken);
}