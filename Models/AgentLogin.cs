using System.ComponentModel.DataAnnotations;

namespace ParcelTrackingSystem.Models
{
    public class AgentLogin
    {
        [Required]
        public string? AgentID {  get; set; }
        [Required]
        public string? Password {  get; set; }
    }
}
