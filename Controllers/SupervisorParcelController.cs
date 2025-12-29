using Microsoft.AspNetCore.Mvc;
using ParcelTrackingSystem.Data;
using System;
using System.Linq;

namespace ParcelTrackingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupervisorParcelController : ControllerBase
    {
        private readonly AppDbContext _context;
        public SupervisorParcelController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("all")]
        public IActionResult GetParcels([FromQuery] string? status, [FromQuery] string? agentId, [FromQuery] DateTime? deliveryDate)
        {
            var query = _context.ParcelCreations.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrEmpty(agentId))
                query = query.Where(p => p.AgentID == agentId);

            if (deliveryDate.HasValue)
                query = query.Where(p => p.DeliveryTime.HasValue &&
                                         p.DeliveryTime.Value.Date == deliveryDate.Value.Date);

            return Ok(query.ToList());
        }

      
        [HttpGet("{parcelId}")]
        public IActionResult GetParcelById(string parcelId)
        {
            var parcel = _context.ParcelCreations.FirstOrDefault(p => p.ParcelID == parcelId);
            if (parcel == null)
                return NotFound("Parcel not found");
            return Ok(parcel);
        }
    }
}
