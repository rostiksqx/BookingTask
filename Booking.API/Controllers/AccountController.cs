using System.Security.Claims;
using AutoMapper;
using Booking.API.Contracts;
using Booking.Core.Enums;
using Booking.Core.Interfaces;
using Booking.Core.JwtResponse;
using Booking.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace Booking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        // Define the necessary services and managers
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        // Inject the services and managers in the constructor
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, 
            IJwtService jwtService, 
            IMapper mapper,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _mapper = mapper;
            _roleManager = roleManager;
        }
       
        // Register a new user
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthenticationResponse>> Register([FromBody] UserRegisterRequest registerRequest)
        {
            // Validate the model
            if (!ModelState.IsValid)
            {
                // Collect and return the validation errors
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                
                string errorMessage = string.Join(", ", errors);
                
                return BadRequest(new { message = errorMessage });
            }

            // Map the request to a User model
            User user = _mapper.Map<User>(registerRequest);
            
            // Create the user
            var result = await _userManager.CreateAsync(user, registerRequest.Password);
            
            // If the creation failed, return the errors
            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Select(e => e.Description);
                
                string errorMessage = string.Join(", ", errors);
                
                return BadRequest(new { message = errorMessage });
            }
            
            // If the role doesn't exist, create it
            if (await _roleManager.FindByNameAsync(UserRoleOptions.User.ToString()) is null)
            {
                var applicationRole = new IdentityRole<Guid>()
                {
                    Name = UserRoleOptions.User.ToString(),
                };
                await _roleManager.CreateAsync(applicationRole);
            }
            // Add the user to the role
            await _userManager.AddToRoleAsync(user, UserRoleOptions.User.ToString());

            // Generate the JWT token
            AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);
            
            // Save the refresh token in the user
            user.RefreshToken = authenticationResponse.RefreshToken;
            user.RefreshTokenExpiryTime = authenticationResponse.RefreshTokenExpiration;
            await _userManager.UpdateAsync(user);

            // Return the authentication response
            return Ok(authenticationResponse);
        }
        
        // Login a user
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] UserLoginRequest userLoginRequest)
        {
            // Validate the model
            if (!ModelState.IsValid)
            {
                // Collect and return the validation errors
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                string errorMessage = string.Join(", ", errors);

                return BadRequest(new { message = errorMessage });
            }

            // Find the user by username or email
            var user = await _userManager.FindByNameAsync(userLoginRequest.EmailOrUsername) ?? await _userManager.FindByEmailAsync(userLoginRequest.EmailOrUsername);

            // If the user doesn't exist, return an error
            if (user == null)
            {
                return BadRequest(new { message = "Invalid credentials" });
            }

            // Check the password
            var result = await _signInManager.CheckPasswordSignInAsync(user, userLoginRequest.Password, false);

            // If the password is incorrect, return an error
            if (!result.Succeeded)
            {
                return Problem("Invalid credentials");
            }

            // Generate the JWT token
            AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);

            // Save the refresh token in the user
            user.RefreshToken = authenticationResponse.RefreshToken;
            user.RefreshTokenExpiryTime = authenticationResponse.RefreshTokenExpiration;
            await _userManager.UpdateAsync(user);

            // Return the authentication response
            return Ok(authenticationResponse);
        }
        
        // Logout a user
        [Authorize]
        [HttpGet("logout")]
        public async Task<ActionResult> Logout()
        {
            // Sign out the user
            await _signInManager.SignOutAsync();

            return Ok();
        }

        // Check if an email is already registered
        private async Task<IActionResult> IsEmailAlreadyRegistered(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                return Ok(true);
            }

            return Ok(true);
        }

        // Generate a new access token
        [HttpPost("generateRefreshToken")]
        public async Task<IActionResult> GenerateNewAccessToken(TokenModel tokenModel)
        {
            // Validate the model
            if (tokenModel is null)
            {
                return BadRequest("Token model is null");
            }

            // Get the claims from the expired token
            ClaimsPrincipal? claimsPrincipal = _jwtService.GetPrincipalFromExpiredToken(tokenModel.Token);

            // If the token is invalid, return an error
            if (claimsPrincipal == null)
            {
                return BadRequest("Invalid token");
            }

            // Get the email from the claims
            string? email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);

            // Find the user by email
            var user = await _userManager.FindByEmailAsync(email);

            // If the user doesn't exist or the refresh token is invalid, return an error
            if (user is null
                || user.RefreshToken != tokenModel.RefreshToken
                || user.RefreshTokenExpiryTime  <= DateTime.Now)
            {
                return BadRequest("Invalid refresh token");
            }

            // Generate a new JWT token
            AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);

            // Save the new refresh token in the user
            user.RefreshToken = authenticationResponse.RefreshToken;
            user.RefreshTokenExpiryTime = authenticationResponse.RefreshTokenExpiration;

            // Update the user
            await _userManager.UpdateAsync(user);

            // Return the new authentication response
            return Ok(authenticationResponse);
        }
    }
}
