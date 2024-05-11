using Booking.Core.Interfaces;
using Booking.Core.Models;

namespace Booking.Core.Services;

public class HousingService : IHousingService
{
    private readonly IHousingRepository _housingRepository;

    public HousingService(IHousingRepository housingRepository)
    {
        _housingRepository = housingRepository;
    }
    
    public async Task<List<Housing>> GetAll()
    {
        return await _housingRepository.GetAll();
    }
    
    public async Task<Housing?> GetById(Guid id)
    {
        return await _housingRepository.GetById(id);
    }
    
    public async Task<Housing> Create(Housing housing)
    {
        return await _housingRepository.Add(housing);
    }
    
    public async Task<Housing> Update(Housing housing)
    {
        return await _housingRepository.Update(housing);
    }
    
    public async Task Delete(Housing housing)
    {
        await _housingRepository.Delete(housing);
    }
}