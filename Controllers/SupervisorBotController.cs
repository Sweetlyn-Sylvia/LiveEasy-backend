using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParcelTrackingSystem.Data;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;

namespace ParcelTrackingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupervisorBotController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string GEMINI_API_KEY = "AIzaSyDnpSmnL6OFJuVY6cf3Nn5bVEh4DZsRWnk";

        public SupervisorBotController(
            AppDbContext context,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

       
        [HttpPost("chat/{supervisorId}")]
        public async Task<IActionResult> Chat(
            string supervisorId,
            [FromBody] BotRequest request)
        {
            try
            {
                var supervisor = await _context.Supervisors
                    .FirstOrDefaultAsync(s => s.SupervisorID == supervisorId);

                if (supervisor == null)
                    return NotFound("Supervisor not found");

                string question = request.Message?.ToLower() ?? "";
                DateTime today = DateTime.Today;
                DateTime now = DateTime.Now;

                var parcels = await _context.ParcelCreations.ToListAsync();
                var agents = await _context.Agents.ToListAsync();

              
                int delivered = parcels.Count(p => p.Status == "Delivered");
                int inTransit = parcels.Count(p => p.Status == "In Transit");
                int pickedUp = parcels.Count(p => p.Status == "Picked Up");

                int agentcount = agents.Count();

              
                double totalEarnings = parcels.Sum(p => p.DeliveryAmount);
                double todaysEarnings = parcels
                    .Where(p => p.Date.Date == today)
                    .Sum(p => p.DeliveryAmount);

             
                int pendingLeaves = await _context.LeaveRequests
                    .CountAsync(l => l.Status == "Pending");

              
                var delayedParcels = parcels
                    .Where(p =>
                        p.Status != "Delivered" &&
                        (now - p.Date).TotalHours >= 48)
                    .Select(p => p.ParcelID)
                    .ToList();

                
                var agentStats = agents.Select(a =>
                {
                    int deliveredCount = parcels.Count(p =>
                        p.AgentID == a.AgentID && p.Status == "Delivered");

                    int inTransitCount = parcels.Count(p =>
                        p.AgentID == a.AgentID && p.Status == "In Transit");

                    int pickedUpCount = parcels.Count(p =>
                        p.AgentID == a.AgentID && p.Status == "Picked Up");

                    int totalParcels = deliveredCount + inTransitCount + pickedUpCount;

                    double efficiency = totalParcels == 0
                        ? 0
                        : (double)deliveredCount / totalParcels * 100;

                    return new
                    {
                        a.AgentID,
                        a.Name,
                        Delivered = deliveredCount,
                        InTransit = inTransitCount,
                        PickedUp = pickedUpCount,
                        Efficiency = Math.Round(efficiency, 1)
                    };
                }).ToList();

                int maxDelivered = agentStats.Max(a => a.Delivered);
                if (maxDelivered == 0) maxDelivered = 1;

                var scoredAgents = agentStats.Select(a =>
                {
                    double volumeScore = (double)a.Delivered / maxDelivered * 100;
                    double finalScore = (a.Efficiency * 0.7) + (volumeScore * 0.3);

                    return new
                    {
                        a.AgentID,
                        a.Name,
                        a.Delivered,
                        a.Efficiency,
                        Score = Math.Round(finalScore, 0)
                    };
                }).ToList();

                var topAgents = scoredAgents
                    .OrderByDescending(a => a.Score)
                    .Take(3)
                    .ToList();

               
                bool askTopAgents = question.Contains("top") || question.Contains("best");
                bool askParcel = question.Contains("parcel") || question.Contains("status");
                bool askEarnings = question.Contains("earning") || question.Contains("amount");
                bool askLeaves = question.Contains("leave");
                bool askDelay = question.Contains("delay");
                bool askagent = question.Contains("number of agent") || question.Contains("how many agents");

              
                StringBuilder context = new StringBuilder();
                context.AppendLine($"Supervisor: {supervisor.Name}");
                context.AppendLine($"Question: {request.Message}");

                if (askTopAgents)
                {
                    context.AppendLine("\n🏆 Top 3 Performing Agents:");
                    foreach (var a in topAgents)
                    {
                        context.AppendLine(
                            $"- {a.Name} ({a.AgentID}) → " +
                            $"Score: {a.Score}%, Efficiency: {a.Efficiency}%, Deliveries: {a.Delivered}"
                        );
                    }
                }
                if (askagent)
                {
                    context.AppendLine($"Number of agents are {agentcount}");
                }

                if (askParcel)
                {
                    context.AppendLine("\n📦 Parcel Overview:");
                    context.AppendLine($"Delivered: {delivered}");
                    context.AppendLine($"In Transit: {inTransit}");
                    context.AppendLine($"Picked Up: {pickedUp}");
                }

                if (askEarnings)
                {
                    context.AppendLine("\n💰 Earnings:");
                    context.AppendLine($"Today's Earnings: ₹{todaysEarnings}");
                    context.AppendLine($"Total Earnings: ₹{totalEarnings}");
                }

                if (askLeaves)
                {
                    context.AppendLine("\n📝 Leave Requests:");
                    context.AppendLine($"Pending Approvals: {pendingLeaves}");
                }

                if (askDelay)
                {
                    context.AppendLine("\n⚠️ Delayed Parcels (>48 hrs):");
                    foreach (var id in delayedParcels)
                        context.AppendLine($"- Parcel {id}");
                }

                string botReply = await CallGemini(context.ToString());

                return Ok(new { reply = botReply });
            }
            catch (Exception ex)
            {
                Console.WriteLine("SUPERVISOR BOT ERROR: " + ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

      
        private async Task<string> CallGemini(string prompt)
        {
            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={GEMINI_API_KEY}";

            var client = _httpClientFactory.CreateClient();

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var response = await client.PostAsJsonAsync(url, body);

            if (!response.IsSuccessStatusCode)
                return "AI service unavailable.";

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();

            return result?.candidates?
                       .FirstOrDefault()?
                       .content?
                       .parts?
                       .FirstOrDefault()?
                       .text
                   ?? "No response generated.";
        }
    }


    public class BotRequest
    {
        public string Message { get; set; }
    }

    public class GeminiResponse
    {
        public GeminiCandidate[] candidates { get; set; }
    }

    public class GeminiCandidate
    {
        public GeminiContent content { get; set; }
    }

    public class GeminiContent
    {
        public GeminiPart[] parts { get; set; }
    }

    public class GeminiPart
    {
        public string text { get; set; }
    }
}
