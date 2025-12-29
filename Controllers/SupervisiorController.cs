using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParcelTrackingSystem.Data;
using ParcelTrackingSystem.Models;
using System.Linq;

namespace ParcelTrackingSystem.Controllers

{
    [ApiController]
    [Route("api/[controller]")]
    public class SupervisiorController : ControllerBase
    {
        private readonly AppDbContext _context;
        public SupervisiorController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] Supervisor loginData)
        {
            var supervisor = _context.Supervisors.FirstOrDefault(s => s.SupervisorID == loginData.SupervisorID && s.Password == loginData.Password);
            if (supervisor == null)
            {
                return Unauthorized("Invalid supervisor ID or password");

            }
            return Ok(new
            {
                Message = "Login Successful",
                SupervisorID = loginData.SupervisorID,
                Name = supervisor.Name ?? "Supervisor",
                Email = supervisor.Email,
                Phone = supervisor.Phone
            });
        }

       
        [HttpGet("dashboard-summary")]
        public IActionResult GetDashboardSummary()
        {
            var today = DateTime.Today;

           
            var totalParcels = _context.ParcelCreations
                .Count(p => p.Date.Date == today);

            
            var deliveredParcels = _context.ParcelCreations
                .Count(p => p.Status == "Delivered" && p.Date.Date == today);

         
            var inTransitParcels = _context.ParcelCreations
                .Count(p => p.Status == "In Transit" && p.Date.Date == today);

          
            var pendingParcels = _context.ParcelCreations
                .Count(p => p.Status == "Picked Up" || p.Status=="In Transit");

         
            var totalAgents = _context.Agents.Count();
            var todaysEarnings = _context.ParcelCreations
    .Where(p =>  p.Date.Date == today)
    .Sum(p => p.DeliveryAmount);

            return Ok(new
            {
                TotalParcels = totalParcels,
                DeliveredParcels = deliveredParcels,
                InTransitParcels = inTransitParcels,
                PendingParcels = pendingParcels,
                TotalAgents = totalAgents,
                TodaysEarnings = todaysEarnings
            });
        }


        [HttpGet("{supervisorID}")]
        public IActionResult GetProfile(string supervisorID)
        {
            var supervisor = _context.Supervisors.FirstOrDefault(s => s.SupervisorID == supervisorID);

            if (supervisor == null)
                return NotFound("Supervisor not found.");

            return Ok(supervisor);
        }

        [HttpPut("update-profile")]
        public IActionResult UpdateProfile([FromBody] ChangePassword dto)
        {
            var supervisor = _context.Supervisors.FirstOrDefault(s => s.SupervisorID == dto.SupervisorID);

            if (supervisor == null)
                return NotFound("Supervisor not found.");

            if (!string.IsNullOrWhiteSpace(dto.CurrentPassword) &&
                !string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                if (supervisor.Password != dto.CurrentPassword)
                    return BadRequest("Current password is incorrect.");

                supervisor.Password = dto.NewPassword;
            }

          
            supervisor.Name = dto.Name;
            supervisor.Email = dto.Email;
            supervisor.Phone = dto.Phone;

            _context.SaveChanges();

            return Ok(new { message = "Profile updated successfully!" });
        }
        [HttpGet("notifications")]
        public IActionResult GetNotifications()
        {
            var today = DateTime.Now;
            var notifications = new List<NotificationResponse>();

         
            var delayed = _context.ParcelCreations
                .Where(p => p.Status != "Delivered" &&
                            EF.Functions.DateDiffHour(p.Date, today) >= 48)
                .ToList();

            foreach (var p in delayed)
            {
                notifications.Add(new NotificationResponse
                {
                    ParcelID = p.ParcelID,
                    Status = p.Status,
                    AgentID = p.AgentID,
                    Type = "Delayed",
                    Message = $"Parcel {p.ParcelID} is delayed for more than 2 days"
                });
            }

            var unassigned = _context.ParcelCreations
    .Where(p =>
        p.AgentID == null &&
        (p.Status == "Picked Up" || p.Status == "In Transit"))
    .ToList();

            foreach (var p in unassigned)
            {
                notifications.Add(new NotificationResponse
                {
                    ParcelID = p.ParcelID,
                    Status = p.Status,
                    AgentID = null,
                    Type = "Unassigned",
                    Message = $"Parcel {p.ParcelID} has no agent assigned"
                });
            }


            return Ok(notifications);
        }

        [HttpGet("agents")]
        public IActionResult GetAllAgents()
        {
            var agents = _context.Agents
                .Select(a => new { a.AgentID, a.Name, a.Email })
                .ToList();

            return Ok(agents);
        }

        public class ReminderRequest
        {
            public string? ParcelID { get; set; }
            public string? AgentID { get; set; }
        }

        [HttpPost("send-reminder")]
        public IActionResult SendReminder([FromBody] ReminderRequest req)
        {
            var agent = _context.Agents
                .FirstOrDefault(a => a.AgentID == req.AgentID);

            if (agent == null)
                return BadRequest("Agent not found");

            string subject = "Parcel Reminder Notification";
            string body = $"Dear {agent.Name},\n\nParcel {req.ParcelID} requires your attention.\nPlease update the status.";

            ParcelTrackingSystem.Services.EmailService.SendEmail(agent.Email, subject, body);

            return Ok(new { message = "Reminder email sent successfully!" });
        }
        [HttpPost("assign-agent")]
        public IActionResult AssignAgent([FromBody] ReminderRequest req)
        {
            var parcel = _context.ParcelCreations.FirstOrDefault(p => p.ParcelID == req.ParcelID);

            if (parcel == null)
                return BadRequest("Parcel not found");

            var agent = _context.Agents.FirstOrDefault(a => a.AgentID == req.AgentID);

            if (agent == null)
                return BadRequest("Agent not found");

            parcel.AgentID = req.AgentID;   
            _context.SaveChanges();

            return Ok(new { message = $"Parcel {req.ParcelID} assigned to Agent {req.AgentID}" });
        }



    }
}

  
