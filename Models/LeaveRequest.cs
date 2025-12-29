using System;
using System.ComponentModel.DataAnnotations;

namespace ParcelTrackingSystem.Models
{
    public class LeaveRequest
    {
        [Key]
        public int LeaveID { get; set; }

        public string AgentID { get; set; }
        [Required]
        public string AgentName { get; set; }
        public DateTime LeaveDate { get; set; }

        public string Reason { get; set; }

        public string Status { get; set; } = "Pending"; 

        public DateTime AppliedOn { get; set; } = DateTime.Now;
    }
}
