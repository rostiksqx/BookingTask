namespace Booking.API.Contracts;

public class UserResponse
{
    public Guid Id { get; set; }
    
    public string Username { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string Token { get; set; } = string.Empty;
    
    public DateTime TokenExpiration { get; set; }
}