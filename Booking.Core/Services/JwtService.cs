using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Booking.Core.Interfaces;
using Booking.Core.JwtResponse;
using Booking.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Booking.Core.Services
{
    public class JwtService : IJwtService
    {
        // Define the necessary configuration
        private readonly IConfiguration _configuration;

        // Inject the configuration in the constructor
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Create a JWT token for a user
        public AuthenticationResponse CreateJwtToken(User user)
        {
            // Define the expiration date of the token
            var expiration = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:EXPIRATION_DAYS"]));

            // Define the claims of the token
            Claim[] claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email.ToString()),
                new Claim(ClaimTypes.Name, user.UserName.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            // Define the security key of the token
            SymmetricSecurityKey securityKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            // Define the signing credentials of the token
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Define the token generator
            JwtSecurityToken tokenGenerator = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expiration,
                signingCredentials: signingCredentials
            );

            // Define the token handler
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            string token = tokenHandler.WriteToken(tokenGenerator);

            // Return the authentication response
            return new AuthenticationResponse
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Token = token,
                Expiration = expiration,
                RefreshToken = GenerateRefreshToken(),
                RefreshTokenExpiration = DateTime.Now.AddDays(Convert.ToDouble(_configuration["RefreshToken:EXPIRATION_DAYS"]))
            };
        }

        // Get the claims principal from an expired token
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            // Define the token validation parameters
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateLifetime = false
            };

            // Define the token handler
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            // Validate the token and get the claims principal
            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            // If the token is not a JWT token or the algorithm is not HS256, throw an exception
            if (securityToken is not JwtSecurityToken jwtSecurityToken
                || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            // Return the claims principal
            return principal;
        }

        // Generate a refresh token
        private string GenerateRefreshToken()
        {
            // Define the byte array
            byte[] bytes = new byte[64];

            // Define the random number generator
            var randomNumberGenerator = RandomNumberGenerator.Create();

            // Fill the byte array with random numbers
            randomNumberGenerator.GetBytes(bytes);

            // Return the base64 string of the byte array
            return Convert.ToBase64String(bytes);
        }
    }
}