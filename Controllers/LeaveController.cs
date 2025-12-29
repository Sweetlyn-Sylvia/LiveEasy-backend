using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParcelTrackingSystem.Data;
using ParcelTrackingSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ParcelTrackingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LeaveController(AppDbContext context)
        {
            _context = context;
        }


        [HttpPost("apply")]
        public async Task<IActionResult> ApplyLeave(LeaveRequest leave)
        {
            leave.Status = "Pending";
            leave.AppliedOn = DateTime.Now;

            _context.LeaveRequests.Add(leave);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Leave applied successfully" });
        }



        [HttpGet("all")]
        public async Task<IActionResult> GetAllLeaves()
        {
            var leaves = await _context.LeaveRequests
                .OrderByDescending(l => l.AppliedOn)
                .ToListAsync();
            return Ok(leaves);
        }

       
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateLeaveStatus(int leaveID, string status)
        {
            var leave = await _context.LeaveRequests.FindAsync(leaveID);
            if (leave == null) return NotFound();

            leave.Status = status;
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Leave {status}" });
        }

        
        [HttpGet("agent/{agentID}")]
        public async Task<IActionResult> GetAgentLeaves(string agentID)
        {
            var leaves = await _context.LeaveRequests
                .Where(l => l.AgentID == agentID)
                .OrderByDescending(l => l.AppliedOn)
                .ToListAsync();
            return Ok(leaves);
        }
        [HttpGet("pending-count")]
        public async Task<IActionResult> GetPendingLeaveCount()
        {
            var count = await _context.LeaveRequests
                                      .CountAsync(l => l.Status == "Pending");

            return Ok(count);
        }
    }
}
