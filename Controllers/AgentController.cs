using Microsoft.AspNetCore.Mvc;
using ParcelTrackingSystem.Data;
using ParcelTrackingSystem.Models;

namespace ParcelTrackingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AgentController(AppDbContext context)
        {
            _context = context;
        }

       
        [HttpPost("signup")]
        public IActionResult SignUp([FromBody] AgentSignUp signupData)
        {
            
            if (_context.Agents.Any(a => a.Email == signupData.Email))
            {
                return BadRequest("Email already exists.");
            }

           
            if (signupData.Password != signupData.ConfirmPassword)
            {
                return BadRequest("Passwords do not match.");
            }

          
            string newAgentId = "AGT" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(signupData.Password);


            var agent = new Agent
            {
                Name = signupData.Name,
                Email = signupData.Email,
                Mobile = signupData.Mobile,
                Password = hashedPassword,
                ConfirmPassword = signupData.ConfirmPassword,
                AgentID = newAgentId
            };

            _context.Agents.Add(agent);
            _context.SaveChanges();

            return Ok(new
            {
                Message = "Signup successful",
                AgentID = agent.AgentID
            });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] AgentLogin loginData)
        {
            var agent = _context.Agents
                .FirstOrDefault(a => a.AgentID == loginData.AgentID );

            if (agent == null)
            {
                return Unauthorized("Invalid AgentID or Password.");
            }
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginData.Password, agent.Password);
            if (!isPasswordValid)
                return Unauthorized("Invalid AgentID or password");

            return Ok(new
            {
                Message = "Login successful",
                AgentID = agent.AgentID,
                Name = agent.Name
            });
        }
        [HttpGet("performance")]
        public IActionResult GetAgentPerformance()
        {
            var agents = _context.Agents.ToList();
            var parcels = _context.ParcelCreations.ToList();

            var performanceData = agents.Select(a => new
            {
                AgentID = a.AgentID,
                Name = a.Name,
                Delivered = parcels.Count(p => p.AgentID == a.AgentID && p.Status == "Delivered"),
                InTransit = parcels.Count(p => p.AgentID == a.AgentID && p.Status == "In Transit"),
                PickedUp = parcels.Count(p => p.AgentID == a.AgentID && p.Status == "Picked Up")
            }).ToList();

            return Ok(performanceData);
        }
        [HttpGet("dashboard/{agentId}")]
        public IActionResult GetDashboardData(string agentId)
        {
            var parcels = _context.ParcelCreations
                .Where(p => p.AgentID == agentId)
                .ToList();

            var stats = new
            {
                Created = parcels.Count(),
                Delivered = parcels.Count(p => p.Status == "Delivered"),
                Pending = parcels.Count(p => p.Status == "Picked Up" || p.Status == "In Transit")
            };

            

            return Ok(new
            {
                Stats = stats,
                
            });
        }
        [HttpGet("all")]
        public IActionResult GetAllAgents()
        {
            var agents = _context.Agents
                .Select(a => new
                {
                    AgentID = a.AgentID,
                    Name = a.Name,
                    
                })
                .ToList();

            return Ok(agents);
        }
        [HttpDelete("delete/{agentId}")]
        public IActionResult DeleteAgent(string agentId)
        {
            var agent = _context.Agents.FirstOrDefault(a => a.AgentID == agentId);
            if (agent == null)
                return NotFound("Agent not found.");

          
            var relatedParcels = _context.ParcelCreations
                                        .Where(p => p.AgentID == agentId)
                                        .ToList();

            foreach (var parcel in relatedParcels)
            {
                parcel.AgentID = null;
            }

        
            _context.Agents.Remove(agent);

            _context.SaveChanges();

            return Ok(new { message = "Agent deleted and parcels unassigned." });
        }
        [HttpPut("update-profile")]
        public IActionResult UpdateAgentProfile([FromBody] AgentProfileUpdate updateData)
        {
            var agent = _context.Agents.FirstOrDefault(a => a.AgentID == updateData.AgentID);

            if (agent == null)
                return NotFound("Agent not found.");

            
            if (_context.Agents.Any(a => a.Email == updateData.Email && a.AgentID != updateData.AgentID))
                return BadRequest("Email already exists.");

            
            agent.Name = updateData.Name;
            agent.Mobile = updateData.Mobile;
            agent.Email = updateData.Email;

            
            if (!string.IsNullOrEmpty(updateData.CurrentPassword) &&
                !string.IsNullOrEmpty(updateData.NewPassword))
            {
                bool isCurrentPasswordValid =
                    BCrypt.Net.BCrypt.Verify(updateData.CurrentPassword, agent.Password);

                if (!isCurrentPasswordValid)
                    return BadRequest("Current password is incorrect.");

                agent.Password = BCrypt.Net.BCrypt.HashPassword(updateData.NewPassword);
            }

            _context.SaveChanges();

            return Ok("Profile updated successfully");
        }


        [HttpGet("profile/{agentId}")]
        public IActionResult GetAgentProfile(string agentId)
        {
            var agent = _context.Agents
                .Where(a => a.AgentID == agentId)
                .Select(a => new
                {
                    a.AgentID,
                    a.Name,
                    a.Email,
                    a.Mobile
                })
                .FirstOrDefault();

            if (agent == null)
                return NotFound("Agent not found.");

            return Ok(agent);
        }
        [HttpGet("performance-badge/{agentId}")]
        public IActionResult GetPerformanceBadge(string agentId)
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            var parcels = _context.ParcelCreations
                .Where(p => p.AgentID == agentId)
                .ToList();

            var weeklyDelivered = parcels.Count(p => p.Status == "Delivered" && p.Date >= startOfWeek);
            var weeklyTotal = parcels.Count(p => p.Date >= startOfWeek);
            var weeklyPercent = weeklyTotal > 0 ? Math.Round((double)weeklyDelivered / weeklyTotal * 100, 1) : 0;

            var monthlyDelivered = parcels.Count(p => p.Status == "Delivered" && p.Date >= startOfMonth);
            var monthlyTotal = parcels.Count(p => p.Date >= startOfMonth);
            var monthlyPercent = monthlyTotal > 0 ? Math.Round((double)monthlyDelivered / monthlyTotal * 100, 1) : 0;

           
            var result = new
            {
                weeklyPercent,
                monthlyPercent,
                weeklyDelivered,
                weeklyTotal,
                monthlyDelivered,
                monthlyTotal,
              
              
            };

            return Ok(result);
        }








    }
}
