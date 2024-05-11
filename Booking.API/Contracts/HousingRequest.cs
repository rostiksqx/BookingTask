﻿namespace Booking.API.Contracts;

public class HousingRequest
{
    public string Name { get; set; }

    public string Description { get; set; }

    public int Rooms { get; set; }

    public string Address { get; set; }

    public bool IsBooked { get; set; }
    
    public Guid UserId { get; set; }
}