using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
namespace API.Services
{
    public class MailSettings
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
    public class MailSender : IEmailSender
    {
        private readonly MailSettings _mailSettings;

        public MailSender(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(_mailSettings.UserName,_mailSettings.Password),
                EnableSsl =true,
                UseDefaultCredentials = false
            };
            return client.SendMailAsync(new MailMessage(_mailSettings.UserName!, email,subject,htmlMessage));
        }
    }
}

