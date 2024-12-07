using Newtonsoft.Json;

namespace DatabaseService.Models.Rabbit;

public class Event
{
    [JsonIgnore]
    public required string NotificationId { get; set; }
    public required string ChannelType { get; set; }
    public required string Recipient { get; set; }
    public required MessageContent MessageContent { get; set; }
    public required string CreatedAt { get; set; }
    public Metadata? Metadata { get; set; }
    public string? Priority { get; set; }
    public required string EventType { get; set; }
}