using System.ComponentModel.DataAnnotations;

namespace BobsBBQApi.BE;

public class LogInDto
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}