using Newtonsoft.Json;

namespace DatabaseService.Models.Rabbit;

public class MessageContent
{
    public string? Subject { get; set; }
    public required string Text { get; set; }
}