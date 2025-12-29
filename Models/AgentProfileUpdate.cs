using System.ComponentModel.DataAnnotations;

namespace ParcelTrackingSystem.Models
{
    public class AgentProfileUpdate
    {
        [Required]
        public string AgentID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Mobile { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
