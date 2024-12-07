using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseService.Models.Postgres;

public class Channel
{
    [Key]
    public int Id { get; set; }
    
    public required string Name { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string? Config { get; set; }
}