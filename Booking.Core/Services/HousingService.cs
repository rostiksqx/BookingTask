using System.IdentityModel.Tokens.Jwt;
using Booking.Core.Interfaces;
using Booking.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace Booking.Core.Services;

public class HousingService : IHousingService
{
    private readonly IHousingRepository _housingRepository;
    private readonly UserManager<User> _userManager;

    public HousingService(IHousingRepository housingRepository, UserManager<User> userManager)
    {
        _housingRepository = housingRepository;
        _userManager = userManager;
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
        if (housing.UserId != null)
        {
            var user = await _userManager.FindByIdAsync(housing.UserId.ToString());
            
            if (user == null)
            {
                return;
            }
            
            user.HousingId = null;
            user.Housing = null;
            await _userManager.UpdateAsync(user);
        }
        
        await _housingRepository.Delete(housing);
    }
    
    public async Task Book(Housing housing, Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
        {
            return;
        }
        
        housing.IsBooked = true;
        housing.UserId = userId;
        housing.User = user;
        
        user.HousingId = housing.Id;
        user.Housing = housing;
        await _userManager.UpdateAsync(user);
        await _housingRepository.Update(housing);
    }

    public async Task UnBook(Housing housing, Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
        {
            return;
        }
        
        housing.IsBooked = false;
        housing.UserId = null;
        housing.User = null;
        
        user!.HousingId = null;
        user.Housing = null;
        await _userManager.UpdateAsync(user);
        await _housingRepository.Update(housing);
    }

    public Guid GetUserIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var userId = jwtToken.Claims.First(claim => claim.Type == "sub").Value;
        
        return Guid.Parse(userId);
    }
}