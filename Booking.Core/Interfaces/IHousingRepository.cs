using Booking.Core.Models;

namespace Booking.Core.Interfaces;

public interface IHousingRepository
{
    Task<List<Housing>> GetAll();
    Task<Housing?> GetById(Guid id);
    Task<Housing> Add(Housing housing);
    Task<Housing> Update(Housing housing);
    Task Delete(Housing housing);
}