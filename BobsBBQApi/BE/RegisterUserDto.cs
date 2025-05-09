using System.ComponentModel.DataAnnotations;

namespace BobsBBQApi.BE;

public class RegisterUserDto
{
    [Required]
    public string UserName { get; set; }
    
    [Required]
    public string Email { get; set; }
    [Required]
    public int PhoneNumber { get; set; }
    [Required]
    public string Password { get; set; } 
    
    [Required]
    public string UserRole { get; set; }
}