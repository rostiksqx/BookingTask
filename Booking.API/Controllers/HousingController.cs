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
        private readonly IHousingService _housingService;
        private readonly IMapper _mapper;

        public HousingController(IHousingService housingService, IMapper mapper)
        {
            _housingService = housingService;
            _mapper = mapper;
        }
        
        
        [HttpGet]
        public async Task<ActionResult<List<HousingResponse>>> GetAll()
        {
            var housings = await _housingService.GetAll();
            
            var housingResponses = _mapper.Map<List<HousingResponse>>(housings);
            
            return Ok(housingResponses);
        }
        
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<HousingResponse>> GetById(Guid id)
        {
            var housing = await _housingService.GetById(id);
            
            if (housing == null)
            {
                return NotFound("Housing not found");
            }
            
            var housingResponse = _mapper.Map<HousingResponse>(housing);
            
            return Ok(housingResponse);
        }
        
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<HousingResponse>> Create(HousingRequest housingRequest)
        {
            var housing = _mapper.Map<Housing>(housingRequest);
            
            var createdHousing = await _housingService.Create(housing);
            
            var housingResponse = _mapper.Map<HousingResponse>(createdHousing);
            
            return CreatedAtAction(nameof(GetById), new { id = housingResponse.Id }, housingResponse);
        }
        
        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<HousingResponse>> Update(Guid id, HousingRequest housingRequest)
        {
            var housing = await _housingService.GetById(id);
            
            if (housing == null)
            {
                return NotFound("Housing not found");
            }
            
            _mapper.Map(housingRequest, housing);
            
            var updatedHousing = await _housingService.Update(housing);
            
            var housingResponse = _mapper.Map<HousingResponse>(updatedHousing);
            
            return Ok(housingResponse);
        }
        
        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var housing = await _housingService.GetById(id);
            
            if (housing == null)
            {
                return NotFound("Housing not found");
            }
            
            await _housingService.Delete(housing);
            
            return NoContent();
        }
        
        [Authorize]
        [HttpPut("{id:guid}/book")]
        public async Task<ActionResult> Book(Guid id)
        {
            string? authHeader = Request.Headers["Authorization"];
            
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                string token = authHeader.Substring("Bearer ".Length).Trim();
                
                Guid userId = _housingService.GetUserIdFromToken(token);
                
                var housing = await _housingService.GetById(id);
                
                if (housing == null)
                {
                    return NotFound("Housing not found");
                }
                
                if (housing.UserId != null)
                {
                    return BadRequest("Housing is already booked");
                }
                
                await _housingService.Book(housing, userId);
                
                return NoContent();
            }
            
            return Unauthorized();
        }
        
        [Authorize]
        [HttpPut("{id:guid}/unBook")]
        public async Task<ActionResult> UnBook(Guid id)
        {
            string? authHeader = Request.Headers["Authorization"];
            
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                string token = authHeader.Substring("Bearer ".Length).Trim();
                
                Guid userId = _housingService.GetUserIdFromToken(token);
                
                var housing = await _housingService.GetById(id);
                
                if (housing == null)
                {
                    return NotFound("Housing not found");
                }
                
                if (housing.UserId != userId)
                {
                    return BadRequest("Housing is not booked by you");
                }
                
                await _housingService.UnBook(housing, userId);
               
                return NoContent();
            }
            
            return Unauthorized();
        }
    }
}
