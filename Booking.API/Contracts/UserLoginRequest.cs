using System.ComponentModel.DataAnnotations;

namespace Booking.API.Contracts;

public class UserLoginRequest
{
    [Required(ErrorMessage = "Email Address can't be blank")]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password can't be blank")]
    public string Password { get; set; } = string.Empty;
}