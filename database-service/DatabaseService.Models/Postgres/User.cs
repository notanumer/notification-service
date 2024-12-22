using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatabaseService.Models.Postgres;

public class User
{
    [Key]
    public int Id { get; set; }
    
    public string? UserName { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string? Credentials { get; set; }
}