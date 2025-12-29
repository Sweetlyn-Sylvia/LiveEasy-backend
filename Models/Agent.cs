using System.ComponentModel.DataAnnotations;

namespace ParcelTrackingSystem.Models
{
    public class Agent
    {
        [Key]
        public int Id { get; set; }
        public string? AgentID { get; set; }


        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Mobile { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public string? ConfirmPassword { get; set; }
    }
}
