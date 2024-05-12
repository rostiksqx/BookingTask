namespace Booking.Core.Models;

public class Housing
{
    public Housing()
    {
        IsBooked = false;
    }
    
    public Guid Id { get; init; }

    public string Name { get; init; }

    public string Description { get; init; }

    public int Rooms { get; init; }

    public string Address { get; init; }

    public bool IsBooked { get; set; }

    public User? User { get; set; }

    public Guid? UserId { get; set; }
}