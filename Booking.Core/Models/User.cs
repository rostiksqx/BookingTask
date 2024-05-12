using Booking.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace Booking.Core.Models;

public class User : IdentityUser<Guid>
{
    public User()
    {
        Role = UserRoleOptions.User;
    }
    
    public string? RefreshToken { get; set; }
    
    public DateTime RefreshTokenExpiryTime { get; set; }
    
    public Guid? HousingId { get; set; }
    
    public Housing? Housing { get; set; }
    
    public UserRoleOptions Role { get; set; }
}