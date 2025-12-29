using System.ComponentModel.DataAnnotations;

namespace ParcelTrackingSystem.Models
{
    public class ChangePassword
    {
        [Required(ErrorMessage = "Supervisor ID is required.")]
        public string? SupervisorID { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone number must be 10 digits.")]
        public string? Phone { get; set; }


        [Required(ErrorMessage = "Current password is required.")]
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$",
            ErrorMessage = "Password must be at least 6 characters long and include uppercase, lowercase, and a number.")]
        public string? NewPassword { get; set; }
    }
}
