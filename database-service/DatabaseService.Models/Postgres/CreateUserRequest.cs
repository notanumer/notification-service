using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace DatabaseService.Models.Postgres;

public class CreateUserRequest
{
    public string? UserName { get; set; }

    [Column(TypeName = "jsonb")]
    public Credentials? Credentials { get; set; }
}