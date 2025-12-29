using Microsoft.AspNetCore.Mvc;
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
    public class AgentBotController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        private const string GEMINI_API_KEY = "AIzaSyDnpSmnL6OFJuVY6cf3Nn5bVEh4DZsRWnk";

        public AgentBotController(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("chat/{agentId}")]
        public async Task<IActionResult> ChatWithBot(string agentId, [FromBody] GeminiChatRequest request)
        {
            var agent = _context.Agents.FirstOrDefault(a => a.AgentID == agentId);
            if (agent == null)
                return NotFound("Agent not found.");

            
            var parcels = _context.ParcelCreations
                .Where(p => p.AgentID == agentId)
                .ToList();

            var pendingParcels = parcels
                .Where(p => p.Status != "Delivered")
                .Select(p => new { p.ParcelID, p.ReceievrName, p.Status, p.DeliveryAmount })
                .ToList();

            var leaves = _context.LeaveRequests
                .Where(l => l.AgentID == agentId)
                .Select(l => new { l.LeaveDate, l.Status, l.Reason })
                .ToList();

            int deliveredCount = parcels.Count(p => p.Status == "Delivered");
            int pendingCount = parcels.Count(p => p.Status != "Delivered");
            int pickedUpCount = parcels.Count(p => p.Status == "Picked Up");
            double totalEarnings = parcels.Sum(p => p.DeliveryAmount);


            // ================= PERFORMANCE =================
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            int weeklyDelivered = parcels.Count(p => p.Status == "Delivered" && p.Date >= startOfWeek);
            int weeklyTotal = parcels.Count(p => p.Date >= startOfWeek);
            double weeklyPercent = weeklyTotal > 0 ? Math.Round((double)weeklyDelivered / weeklyTotal * 100, 1) : 0;

            int monthlyDelivered = parcels.Count(p => p.Status == "Delivered" && p.Date >= startOfMonth);
            int monthlyTotal = parcels.Count(p => p.Date >= startOfMonth);
            double monthlyPercent = monthlyTotal > 0 ? Math.Round((double)monthlyDelivered / monthlyTotal * 100, 1) : 0;

            string performanceLevel =
                weeklyPercent >= 90 ? "🏆 Excellent" :
                weeklyPercent >= 75 ? "⭐ Good" :
                weeklyPercent >= 50 ? "⚠️ Average" : "❌ Needs Improvement";

            // ================= AI CONTEXT =================
            // ================= TODAY EARNINGS =================
            double todayEarnings = parcels
                .Where(p => p.Status == "Delivered" && p.Date.Date == today)
                .Sum(p => p.DeliveryAmount);

            StringBuilder contextText = new StringBuilder();

            contextText.AppendLine($"Agent Name: {agent.Name}");
            contextText.AppendLine($"Agent ID: {agent.AgentID}");

            contextText.AppendLine("\n📦 Parcel Summary:");
            contextText.AppendLine($"Delivered: {deliveredCount}");
            contextText.AppendLine($"Picked Up: {pickedUpCount}");
            contextText.AppendLine($"Pending: {pendingCount}");
            contextText.AppendLine($"Total Earnings: ₹{totalEarnings:F2}");

            contextText.AppendLine("\n📅 Weekly Performance:");
            contextText.AppendLine($"Delivered: {weeklyDelivered}/{weeklyTotal}");
            contextText.AppendLine($"Success Rate: {weeklyPercent}%");
            contextText.AppendLine($"Performance Level: {performanceLevel}");

            contextText.AppendLine("\n📆 Monthly Performance:");
            contextText.AppendLine($"Delivered: {monthlyDelivered}/{monthlyTotal}");
            contextText.AppendLine($"Success Rate: {monthlyPercent}%");

            contextText.AppendLine("\n📝 Leave Requests:");
            foreach (var l in leaves)
                contextText.AppendLine($"- {l.LeaveDate:dd-MM-yyyy}: {l.Status}");
            contextText.AppendLine("\n💰 Earnings Summary:");
            contextText.AppendLine($"Today's Earnings: ₹{todayEarnings:F2}");
            contextText.AppendLine($"Total Earnings: ₹{totalEarnings:F2}");


            contextText.AppendLine($"\n💬 Agent Question: {request.Message}");
            contextText.AppendLine("Answer clearly, politely, and professionally.");

            string botReply = await CallGeminiAI(contextText.ToString());

            return Ok(new { reply = botReply });
        }

        // ================= GEMINI =================
        private async Task<string> CallGeminiAI(string prompt)
        {
            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={GEMINI_API_KEY}";

            var httpClient = _httpClientFactory.CreateClient();

            var requestBody = new
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

            var response = await httpClient.PostAsJsonAsync(url, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return $"Gemini API Error: {error}";
            }

            var result = await response.Content.ReadFromJsonAsync<GeminiApiResponse>();

            return result?.candidates?
                           .FirstOrDefault()?
                           .content?
                           .parts?
                           .FirstOrDefault()?
                           .text
                   ?? "Sorry, I couldn't generate a response.";
        }
    }

    // ================= MODELS =================
    public class GeminiChatRequest
    {
        public string Message { get; set; }
    }

    public class GeminiApiResponse
    {
        public Candidate[] candidates { get; set; }
    }

    public class Candidate
    {
        public Content content { get; set; }
    }

    public class Content
    {
        public Part[] parts { get; set; }
    }

    public class Part
    {
        public string text { get; set; }
    }
}
