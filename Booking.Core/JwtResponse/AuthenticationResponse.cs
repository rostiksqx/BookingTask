﻿namespace Booking.Core.JwtResponse;

public class AuthenticationResponse
{
    public Guid Id { get; set; }
    public string? Username { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Token { get; set; } = string.Empty;
    public DateTime? Expiration { get; set; }
    public string? RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiration { get; set; }
}