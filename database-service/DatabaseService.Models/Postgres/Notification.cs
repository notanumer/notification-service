using System.ComponentModel.DataAnnotations;

namespace DatabaseService.Models.Postgres;

public class Notification
{
    [Key]
    public int Id { get; set; }
    
    public required string MessageType { get; set; }
    
    public required string Recipient { get; set; }
    
    public required string MessageContent { get; set; }
    
    public bool IsSuccess { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
}