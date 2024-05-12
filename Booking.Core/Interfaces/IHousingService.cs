using Booking.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Core.Services;

public interface IHousingService
{
    Task<List<Housing>> GetAll();
    Task<Housing?> GetById(Guid id);
    Task<Housing> Create(Housing housing);
    Task<Housing> Update(Housing housing);
    Task Delete(Housing housing);
    
    Task Book(Housing housing, Guid userId);
    
    Task UnBook(Housing housing, Guid userId);

    Guid GetUserIdFromToken(string token);
}