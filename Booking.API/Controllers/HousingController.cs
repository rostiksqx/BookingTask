using AutoMapper;
using Booking.API.Contracts;
using Booking.Core.Models;
using Booking.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Booking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HousingController : ControllerBase
    {
        // Define the necessary services and mapper
        private readonly IHousingService _housingService;
        private readonly IMapper _mapper;

        // Inject the services and mapper in the constructor
        public HousingController(IHousingService housingService, IMapper mapper)
        {
            _housingService = housingService;
            _mapper = mapper;
        }

        // Get all housings
        [HttpGet]
        public async Task<ActionResult<List<HousingResponse>>> GetAll()
        {
            // Fetch all housings from the service
            var housings = await _housingService.GetAll();

            // Map the housings to the response model
            var housingResponses = _mapper.Map<List<HousingResponse>>(housings);

            // Return the housings
            return Ok(housingResponses);
        }

        // Get a housing by id
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<HousingResponse>> GetById(Guid id)
        {
            // Fetch the housing from the service
            var housing = await _housingService.GetById(id);

            // If the housing doesn't exist, return a not found error
            if (housing == null)
            {
                return NotFound("Housing not found");
            }

            // Map the housing to the response model
            var housingResponse = _mapper.Map<HousingResponse>(housing);

            // Return the housing
            return Ok(housingResponse);
        }

        // Create a new housing
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<HousingResponse>> Create(HousingRequest housingRequest)
        {
            // Map the request to a Housing model
            var housing = _mapper.Map<Housing>(housingRequest);

            // Create the housing using the service
            var createdHousing = await _housingService.Create(housing);

            // Map the created housing to the response model
            var housingResponse = _mapper.Map<HousingResponse>(createdHousing);

            // Return the created housing
            return CreatedAtAction(nameof(GetById), new { id = housingResponse.Id }, housingResponse);
        }

        // Update a housing
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<HousingResponse>> Update(Guid id, HousingRequest housingRequest)
        {
            // Fetch the housing from the service
            var housing = await _housingService.GetById(id);

            // If the housing doesn't exist, return a not found error
            if (housing == null)
            {
                return NotFound("Housing not found");
            }

            // Map the request to the existing housing
            _mapper.Map(housingRequest, housing);

            // Update the housing using the service
            var updatedHousing = await _housingService.Update(housing);

            // Map the updated housing to the response model
            var housingResponse = _mapper.Map<HousingResponse>(updatedHousing);

            // Return the updated housing
            return Ok(housingResponse);
        }

        // Delete a housing
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            // Fetch the housing from the service
            var housing = await _housingService.GetById(id);

            // If the housing doesn't exist, return a not found error
            if (housing == null)
            {
                return NotFound("Housing not found");
            }

            // Delete the housing using the service
            await _housingService.Delete(housing);

            // Return no content
            return NoContent();
        }

        // Book a housing
        [Authorize]
        [HttpPut("{id:guid}/book")]
        public async Task<ActionResult> Book(Guid id)
        {
            // Get the authorization header
            string? authHeader = Request.Headers["Authorization"];

            // If the header is present and starts with "Bearer "
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                // Extract the token from the header
                string token = authHeader.Substring("Bearer ".Length).Trim();

                // Get the user id from the token
                Guid userId = _housingService.GetUserIdFromToken(token);

                // Fetch the housing from the service
                var housing = await _housingService.GetById(id);

                // If the housing doesn't exist, return a not found error
                if (housing == null)
                {
                    return NotFound("Housing not found");
                }

                // If the housing is already booked, return a bad request error
                if (housing.UserId != null)
                {
                    return BadRequest("Housing is already booked");
                }

                // Book the housing using the service
                await _housingService.Book(housing, userId);

                // Return no content
                return NoContent();
            }

            // If the authorization header is not present or doesn't start with "Bearer ", return an unauthorized error
            return Unauthorized();
        }

        // Unbook a housing
        [Authorize]
        [HttpPut("{id:guid}/unBook")]
        public async Task<ActionResult> UnBook(Guid id)
        {
            // Get the authorization header
            string? authHeader = Request.Headers["Authorization"];

            // If the header is present and starts with "Bearer "
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                // Extract the token from the header
                string token = authHeader.Substring("Bearer ".Length).Trim();

                // Get the user id from the token
                Guid userId = _housingService.GetUserIdFromToken(token);

                // Fetch the housing from the service
                var housing = await _housingService.GetById(id);

                // If the housing doesn't exist, return a not found error
                if (housing == null)
                {
                    return NotFound("Housing not found");
                }

                // If the housing is not booked by the user, return a bad request error
                if (housing.UserId != userId)
                {
                    return BadRequest("Housing is not booked by you");
                }

                // UnBook the housing using the service
                await _housingService.UnBook(housing, userId);

                // Return no content
                return NoContent();
            }

            // If the authorization header is not present or doesn't start with "Bearer ", return an unauthorized error
            return Unauthorized();
        }
    }
}