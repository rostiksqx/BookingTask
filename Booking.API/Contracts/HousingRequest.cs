using System.ComponentModel.DataAnnotations;

namespace Booking.API.Contracts;

public class HousingRequest
{
    [Required(ErrorMessage = "Name can't be blank")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Description can't be blank")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Price can't be blank")]
    [Range(0, 30, ErrorMessage = "Price should be greater than 0 ")]
    public int Rooms { get; set; }

    [Required(ErrorMessage = "Address can't be blank")]
    [StringLength(100, ErrorMessage = "Address should be less than 100 characters")]
    public string Address { get; set; }
}