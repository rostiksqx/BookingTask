using System.IdentityModel.Tokens.Jwt;
using Booking.Core.Interfaces;
using Booking.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace Booking.Core.Services
{
    public class HousingService : IHousingService
    {
        // Define the necessary repository and manager
        private readonly IHousingRepository _housingRepository;
        private readonly UserManager<User> _userManager;

        // Inject the repository and manager in the constructor
        public HousingService(IHousingRepository housingRepository, UserManager<User> userManager)
        {
            _housingRepository = housingRepository;
            _userManager = userManager;
        }

        // Get all housings
        public async Task<List<Housing>> GetAll()
        {
            return await _housingRepository.GetAll();
        }

        // Get a housing by id
        public async Task<Housing?> GetById(Guid id)
        {
            return await _housingRepository.GetById(id);
        }

        // Create a new housing
        public async Task<Housing> Create(Housing housing)
        {
            return await _housingRepository.Add(housing);
        }

        // Update a housing
        public async Task<Housing> Update(Housing housing)
        {
            return await _housingRepository.Update(housing);
        }

        // Delete a housing
        public async Task Delete(Housing housing)
        {
            // If the housing is booked
            if (housing.UserId != null)
            {
                // Fetch the user from the manager
                var user = await _userManager.FindByIdAsync(housing.UserId.ToString());

                // If the user doesn't exist, return
                if (user == null)
                {
                    return;
                }

                // Unbook the housing for the user
                user.HousingId = null;
                user.Housing = null;
                await _userManager.UpdateAsync(user);
            }

            // Delete the housing from the repository
            await _housingRepository.Delete(housing);
        }

        // Book a housing
        public async Task Book(Housing housing, Guid userId)
        {
            // Fetch the user from the manager
            var user = await _userManager.FindByIdAsync(userId.ToString());

            // If the user doesn't exist, return
            if (user == null)
            {
                return;
            }

            // Book the housing for the user
            housing.IsBooked = true;
            housing.UserId = userId;
            housing.User = user;

            // Update the user's housing
            user.HousingId = housing.Id;
            user.Housing = housing;
            await _userManager.UpdateAsync(user);
            await _housingRepository.Update(housing);
        }

        // Unbook a housing
        public async Task UnBook(Housing housing, Guid userId)
        {
            // Fetch the user from the manager
            var user = await _userManager.FindByIdAsync(userId.ToString());

            // If the user doesn't exist, return
            if (user == null)
            {
                return;
            }

            // Unbook the housing for the user
            housing.IsBooked = false;
            housing.UserId = null;
            housing.User = null;

            // Update the user's housing
            user!.HousingId = null;
            user.Housing = null;
            await _userManager.UpdateAsync(user);
            await _housingRepository.Update(housing);
        }

        // Get the user id from a token
        public Guid GetUserIdFromToken(string token)
        {
            // Create a new JWT handler
            var handler = new JwtSecurityTokenHandler();

            // Read the JWT token
            var jwtToken = handler.ReadJwtToken(token);

            // Get the user id from the token
            var userId = jwtToken.Claims.First(claim => claim.Type == "sub").Value;

            // Return the user id
            return Guid.Parse(userId);
        }
    }
}