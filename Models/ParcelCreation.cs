using System.ComponentModel.DataAnnotations;

namespace ParcelTrackingSystem.Models
{
    public class ParcelCreation
    {
        [Key]
        public int Id { get; set; }

        public string? ParcelID { get; set; }

        [Required(ErrorMessage = "Sender Name is required")]
        public string? SenderName { get; set; }

        [Required(ErrorMessage = "Receiver Name is required")]
        public string? ReceievrName { get; set; }

        [Required(ErrorMessage = "Sender Address is required")]
        public string? SenderAddress { get; set; }

        [Required(ErrorMessage = "Receiver Address is required")]
        public string? ReceiverAddress { get; set; }

        [Required(ErrorMessage = "Contact Number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Contact number must be exactly 10 digits")]
        public string? ReceiverContactNumber { get; set; }

      
        [Required(ErrorMessage = "Weight is required")]
        [Range(0.01, 10000, ErrorMessage = "Weight must be greater than 0")]
        public decimal? Weight { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public string? Status { get; set; } = "Picked Up";

        public string? AgentID { get; set; }

        public DateTime? DeliveryTime { get; set; }
        public string? Remarks { get; set; }
        public double DeliveryAmount { get; set; }
        public bool FastDelivery { get; set; }
        public string PaymentMode { get; set; } 
        public bool IsPaid { get; set; }
       

    }
}
