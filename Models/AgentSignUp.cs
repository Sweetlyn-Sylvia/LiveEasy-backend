using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParcelTrackingSystem.Models
{
    public class AgentSignUp
    {
        [Key]
        public int Id { get; set; }

        public string? AgentID { get; set; }

       
        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }

       
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        
        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Mobile number must be exactly 10 digits")]
        public string? Mobile { get; set; }


        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(
           @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
           ErrorMessage = "Password must contain uppercase, lowercase, number, and special character"
       )]
        public string? Password { get; set; }

      
        [NotMapped]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }
    }
}
