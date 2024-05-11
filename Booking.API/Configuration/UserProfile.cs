using AutoMapper;
using Booking.API.Contracts;
using Booking.Core.JwtResponse;
using Booking.Core.Models;

namespace Booking.API.Configuration;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserRegisterRequest, User>();
    }
}