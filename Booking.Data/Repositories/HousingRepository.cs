using Booking.Core.Interfaces;
using Booking.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Booking.Data.Repositories;

public class HousingRepository : IHousingRepository
{
    private readonly BookingDbContext _dbContext;

    public HousingRepository(BookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<Housing>> GetAll()
    {
        return await _dbContext.Housings
            .AsNoTracking()
            .Include(h => h.User)
            .ToListAsync();
    }
    
    public async Task<Housing?> GetById(Guid id)
    {
        return await _dbContext.Housings
            .AsNoTracking()
            .Include(h => h.User)
            .FirstOrDefaultAsync(h => h.Id == id);
    }
    
    public async Task<Housing> Add(Housing housing)
    {
        await _dbContext.Housings.AddAsync(housing);
        await _dbContext.SaveChangesAsync();
        
        return housing;
    }
    
    public async Task<Housing> Update(Housing housing)
    {
        await _dbContext.Housings
            .Where(h => h.Id == housing.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(h => h.Name, h => housing.Name)
                .SetProperty(h => h.Description, h => housing.Description)
                .SetProperty(h => h.Rooms, h => housing.Rooms)
                .SetProperty(h => h.Address, h => housing.Address)
                .SetProperty(h => h.IsBooked, h => housing.IsBooked));

        await _dbContext.SaveChangesAsync();
        
        return housing;
    }
    
    public async Task Delete(Housing housing)
    {
        await _dbContext.Housings
            .Where(h => h.Id == housing.Id)
            .ExecuteDeleteAsync();
        
        await _dbContext.SaveChangesAsync();
    }
}