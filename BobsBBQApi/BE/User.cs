using System.ComponentModel.DataAnnotations;

namespace BobsBBQApi.BE;

public class User
{
    [Key]
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public int PhoneNumber { get; set; }
    public string UserRole { get; set; } 
    public string UserHash { get; set; }
    public string UserSalt { get; set; }
}