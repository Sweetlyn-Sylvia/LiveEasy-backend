using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParcelTrackingSystem.Data;
using ParcelTrackingSystem.Models;
using System;
using System.Linq;

namespace ParcelTrackingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParcelCreationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ParcelCreationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public IActionResult AddParcel([FromBody] ParcelCreation parcelData)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                parcelData.ParcelID = "PAR" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                parcelData.Date = DateTime.Now;
                parcelData.Status = "Picked Up";

                _context.ParcelCreations.Add(parcelData);
                _context.SaveChanges();

                return Ok(new
                {
                    Message = "Parcel Added Successfully",
                    ParcelID = parcelData.ParcelID,
                    Date = parcelData.Date,
                    Amount=parcelData.DeliveryAmount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message,
                    Details = ex.InnerException?.Message
                });
            }
        }


        [HttpGet("all")]
        public async Task<IActionResult> GetAllParcels()
        {
            var parcels = await _context.ParcelCreations.ToListAsync();
            return Ok(parcels);
        }

        [HttpGet("getparcels/{agentID}")]
        public async Task<IActionResult> GetParcels(string agentID)
        {
            var parcels = await _context.ParcelCreations
                .Where(p => p.AgentID == agentID && p.Status != "Delivered")
                .OrderByDescending(p => p.FastDelivery)
                .ThenByDescending(p => p.Date)
                .ToListAsync();

            if (!parcels.Any())
                return NotFound("No parcels found for this agent.");

            return Ok(parcels);
        }




        [HttpGet("{parcelID}")]
        public IActionResult GetParcelById(string parcelID)
        {
            var parcel = _context.ParcelCreations.FirstOrDefault(p => p.ParcelID == parcelID);
            if (parcel == null)
                return NotFound("Parcel not found.");

            return Ok(parcel);
        }
        public class StatusUpdateRequest
        {
            public string NewStatus { get; set; }
            public string? Remarks { get; set; }
            public string? DeliveryImage { get; set; } 
        }

        [HttpPut("update-status/{parcelID}")]
        public IActionResult UpdateStatus(string parcelID, [FromBody] StatusUpdateRequest request)
        {
            var parcel = _context.ParcelCreations.FirstOrDefault(p => p.ParcelID == parcelID);
            if (parcel == null)
                return NotFound("Parcel not found.");

           
            var allowedSequence = new List<string> { "Picked Up", "In Transit", "Delivered" };
            var currentStatusIndex = parcel.Status != null ? allowedSequence.IndexOf(parcel.Status) : -1;
            var newStatusIndex = allowedSequence.IndexOf(request.NewStatus);

           
            if (newStatusIndex == -1)
                return BadRequest("Invalid status value.");

            if (newStatusIndex != currentStatusIndex + 1)
                return BadRequest($"Status must follow the order: Picked Up -> In Transit -> Delivered. Current status is '{parcel.Status ?? "None"}'.");

           
            parcel.Status = request.NewStatus;
            parcel.Remarks = request.Remarks;

         
            parcel.DeliveryTime = request.NewStatus == "Delivered" ? DateTime.Now : null;

            
            if (!string.IsNullOrEmpty(request.DeliveryImage))
            {
                try
                {
                    var base64Data = request.DeliveryImage.Contains(",")
                        ? request.DeliveryImage.Split(',')[1]
                        : request.DeliveryImage;

                    var bytes = Convert.FromBase64String(base64Data);

                    var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(imagesFolder))
                        Directory.CreateDirectory(imagesFolder);

                    var fileName = $"{parcel.ParcelID}_{DateTime.Now:yyyyMMddHHmmss}.png";
                    var filePath = Path.Combine(imagesFolder, fileName);

                    System.IO.File.WriteAllBytes(filePath, bytes);

                    parcel.Remarks += $" | Delivery Image: {fileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error saving image: {ex.Message}");
                }
            }

            _context.SaveChanges();

            return Ok(new
            {
                Message = "Status updated successfully",
                UpdatedStatus = parcel.Status,
                Remarks = parcel.Remarks,
                DeliveryTime = parcel.DeliveryTime
            });
        }




        [HttpGet("pending")]
        public IActionResult GetPendingParcels()
        {
            var pendingParcels = _context.ParcelCreations.Where(p=>p.Status!="Delivered").ToList();
            if (!pendingParcels.Any())
                return NotFound("No pending parcels found");
            return Ok(pendingParcels);
        }
        [HttpGet("manage")]
        public IActionResult GetParcels([FromQuery] string? status, [FromQuery] string? agentId, [FromQuery] DateTime? date)
        {
            var query = _context.ParcelCreations.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            if (!string.IsNullOrEmpty(agentId))
                query = query.Where(p => p.AgentID == agentId);

            if (date.HasValue)
                query = query.Where(p => p.Date.Date == date.Value.Date);

            var result = query.Select(p => new
            {
                p.ParcelID
            }).ToList();

            return Ok(result);
        }
    }
}
