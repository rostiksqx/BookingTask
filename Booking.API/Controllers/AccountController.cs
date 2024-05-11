using System.Security.Claims;
using AutoMapper;
using Booking.API.Contracts;
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
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IJwtService jwtService, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _mapper = mapper;
        }
       
        [HttpPost("register")]
        public async Task<ActionResult<UserResponse>> Register([FromBody] UserRegisterRequest registerRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                
                string errorMessage = string.Join(", ", errors);
                
                return BadRequest(new { message = errorMessage });
            }

            User user = _mapper.Map<User>(registerRequest);
            
            var result = await _userManager.CreateAsync(user, registerRequest.Password);
            
            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .Select(e => e.Description);
                
                string errorMessage = string.Join(", ", errors);
                
                return BadRequest(new { message = errorMessage });
            }

            AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);

            user.RefreshToken = authenticationResponse.RefreshToken;
            user.RefreshTokenExpiryTime = authenticationResponse.RefreshTokenExpiration;
            await _userManager.UpdateAsync(user);
            
            UserResponse userResponse = _mapper.Map<UserResponse>(user);
            
            userResponse.Token = authenticationResponse.Token;
            userResponse.TokenExpiration.ToString("yyyy-MM-ddTHH:mm:ss");
            
            return Ok(userResponse);
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<UserResponse>> Login([FromBody] UserLoginRequest userLoginRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                
                string errorMessage = string.Join(", ", errors);
                
                return BadRequest(new { message = errorMessage });
            }
            
            var user = await _userManager.FindByNameAsync(userLoginRequest.EmailOrUsername) ?? await _userManager.FindByEmailAsync(userLoginRequest.EmailOrUsername);
            
            if (user == null)
            {
                return BadRequest(new { message = "Invalid credentials" });
            }
            
            var result = await _signInManager.CheckPasswordSignInAsync(user, userLoginRequest.Password, false);
            
            if (!result.Succeeded)
            {
                return Problem("Invalid credentials");
            }
            
            AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);
            
            user.RefreshToken = authenticationResponse.RefreshToken;
            user.RefreshTokenExpiryTime = authenticationResponse.RefreshTokenExpiration;
            await _userManager.UpdateAsync(user);
            
            UserResponse userResponse = _mapper.Map<UserResponse>(user);
            
            return Ok(userResponse);
        }
        
        [HttpGet("logout")]
        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            
            return Ok();
        }
        
        [HttpGet]
        public async Task<IActionResult> IsEmailAlreadyRegistered(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            
            if (user != null)
            {
                return Ok(true);
            }
            
            return Ok(true);
        }

        [HttpPost("generateRefreshToken")]
        public async Task<IActionResult> GenerateNewAccessToken(TokenModel tokenModel)
        {
            if (tokenModel is null)
            {
                return BadRequest("Token model is null");
            }
            
            ClaimsPrincipal? claimsPrincipal = _jwtService.GetPrincipalFromExpiredToken(tokenModel.Token);
            
            if (claimsPrincipal == null)
            {
                return BadRequest("Invalid token");
            }
            
            string? email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);
            
            var user = await _userManager.FindByEmailAsync(email);
            
            if (user is null 
                || user.RefreshToken != tokenModel.RefreshToken
                || user.RefreshTokenExpiryTime  <= DateTime.Now)
            {
                return BadRequest("Invalid refresh token");
            }
            
            AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);
            
            user.RefreshToken = authenticationResponse.RefreshToken;
            user.RefreshTokenExpiryTime = authenticationResponse.RefreshTokenExpiration;
            
            await _userManager.UpdateAsync(user);
            
            return Ok(authenticationResponse);
        }
    }
}
