using System.Net;
using System.Net.Mail;

namespace ParcelTrackingSystem.Services
{
    public class EmailService
    {
        public static void SendEmail(string to, string subject, string body)
        {
            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("liveeasynotification@gmail.com", "alaf qexf snhw ddlm"),
                EnableSsl = true
            };

            smtp.Send("liveeasynotification@gmail.com", to, subject, body);
        }
    }
}
