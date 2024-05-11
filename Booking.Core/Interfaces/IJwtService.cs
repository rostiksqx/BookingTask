using System.Security.Claims;
using Booking.Core.JwtResponse;
using Booking.Core.Models;

namespace Booking.Core.Interfaces;

public interface IJwtService
{
    AuthenticationResponse CreateJwtToken(User user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);
}