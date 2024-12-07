using Newtonsoft.Json;

namespace DatabaseService.Models.Rabbit;

public class Metadata
{
    public string? Subject { get; set; }
    public required string Body { get; set; }
}