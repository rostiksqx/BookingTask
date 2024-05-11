namespace Booking.API.Contracts;

public class HousingResponse
{
    public Guid Id { get; set; }
    
    public string Name { get; init; }

    public string Description { get; init; }

    public int Rooms { get; init; }

    public string Address { get; init; }

    public bool IsBooked { get; init; }
    
    public Guid? UserId { get; init; }
}