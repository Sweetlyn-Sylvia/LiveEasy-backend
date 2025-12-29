namespace ParcelTrackingSystem.Models
{
    public class NotificationResponse
    {
        public string? ParcelID { get; set; }
        public string? Status { get; set; }
        public string? AgentID { get; set; }
        public string? Type { get; set; }  
        public string? Message { get; set; }
    }
}
