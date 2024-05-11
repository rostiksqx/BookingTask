namespace Booking.Core.Models;

public class Housing
{
    public Guid Id { get; init; }

    public string Name { get; init; }

    public string Description { get; init; }

    public int Rooms { get; init; }

    public string Address { get; init; }

    public bool IsBooked { get; init; }

    public User? User { get; init; }

    public Guid? UserId { get; init; }
}