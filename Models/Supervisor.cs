using System.ComponentModel.DataAnnotations;

namespace ParcelTrackingSystem.Models
{
    public class Supervisor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? SupervisorID { get; set; }

        [Required]
        public string? Password { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
