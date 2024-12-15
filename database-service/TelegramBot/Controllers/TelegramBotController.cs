using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using TelegramBot.Services;

namespace DatabaseService.Controllers;

[ApiController]
[Route("bot")]
public class TelegramBotController : ControllerBase
{
    private readonly TelegramBotNotifyService _service;

    public TelegramBotController(TelegramBotNotifyService service)
    {
        _service = service;
    }

    [HttpGet("getOne")]
    public string Get()
    {
        return "1";
    }

    [HttpPost("send/{userId}")]
    public async Task SendEvent([FromQuery] string text, [FromRoute] long userId)
    {
        await _service.SendMessage(userId, text);
    }
}