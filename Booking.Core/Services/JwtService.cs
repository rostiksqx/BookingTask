using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Booking.Core.Interfaces;
using Booking.Core.JwtResponse;
using Booking.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Booking.Core.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public AuthenticationResponse CreateJwtToken(User user)
    {
        var expirationTime = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:ExpirationDays"));

        Claim[] claims = new Claim[]
        {
            new Claim("Id", user.Id.ToString()),
            new Claim("Email", user.Email.ToString()),
            new Claim("UserName", user.UserName.ToString()),
        };
        
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SymmetricSecurityKey"]));
        
        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        JwtSecurityToken tokenGenerator = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: expirationTime,
            signingCredentials: credentials
        );
        
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        string token = tokenHandler.WriteToken(tokenGenerator);

        return new AuthenticationResponse
        {
            Username = user.UserName,
            Email = user.Email,
            Token = token,
            RefreshToken = GenerateRefreshToken(),
            RefreshTokenExpiration = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:ExpirationDays"))
        };
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
    {
        var tokenValidationParameters = new TokenValidationParameters()
        {
            ValidateAudience = true,
            ValidAudience = _configuration["Jwt:Audience"],
            ValidateIssuer = true,
            ValidIssuer = _configuration["Jwt:Issuer"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SymmetricSecurityKey"])),
            ValidateLifetime = false
        };
        
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        
        try
        {
            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken 
                || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            
            return principal;
        }
        catch
        {
            return null;
        }
    }
    
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}