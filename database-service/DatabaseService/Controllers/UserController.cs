using System.Diagnostics.Metrics;
using DatabaseService.Models.Postgres;
using DatabaseService.Models.Rabbit;
using DatabaseService.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseService.Controllers;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly Counter<int> _userCreate;

    public UserController(IUserService userService, IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("notifications");
        _userCreate = meter.CreateCounter<int>("user_create");
        _userService = userService;
    }

    [HttpPost("create")]
    public async Task CreateUser([FromBody] CreateUserRequest request)
    {
        _userCreate.Add(1, new KeyValuePair<string, object?>("name", request.UserName));
        await _userService.CreateUser(request);
    }

    [HttpPatch("setCredentials/{userName}")]
    public async Task setCredentials([FromRoute] string userName, [FromBody] Credentials credentials)
    {
        await _userService.PatchCredentials(userName, credentials);
    }

    [HttpGet("getCredentials/{userName}/{credentialType}")]
    public async Task<IActionResult> GetCredentials([FromRoute] string userName, [FromRoute] ChannelType credentialType)
    {
        var credentials = await _userService.GetCredentials(userName, credentialType);
        if (credentials == null) return NotFound();
        return Ok(new { 
            credentials = credentials
        });
    }
}