using Microsoft.AspNetCore.Identity;

namespace Booking.Core.Models;

public class User : IdentityUser<Guid>
{
    public string? RefreshToken { get; set; }
    
    public DateTime RefreshTokenExpiryTime { get; set; }
    
    public ICollection<Housing> Housings { get; set; } = new List<Housing>();
}