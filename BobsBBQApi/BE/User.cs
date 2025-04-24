using System.ComponentModel.DataAnnotations;

namespace BobsBBQApi.BE;

public class User
{
    [Key]
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public int PhoneNumber { get; set; }
    public int UserRole { get; set; } // 0 = customer, 1 = admin
    public string UserHash { get; set; }
    public string UserSalt { get; set; }
}