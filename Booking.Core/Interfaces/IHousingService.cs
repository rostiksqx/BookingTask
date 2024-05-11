using Booking.Core.Models;

namespace Booking.Core.Services;

public interface IHousingService
{
    Task<List<Housing>> GetAll();
    Task<Housing?> GetById(Guid id);
    Task<Housing> Create(Housing housing);
    Task<Housing> Update(Housing housing);
    Task Delete(Housing housing);
}