using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Api.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly EmailSettings _emailSettings;


        public EmailService(IConfiguration config, IOptions<EmailSettings> emailSettings)
        {
            _config = config;
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            if (string.IsNullOrEmpty(_config["EmailSettings:SenderEmail"]))
            {
                throw new ArgumentException("Sender email is not configured.");
            }


            var smtpClient = new SmtpClient(_config["EmailSettings:SmtpServer"])
            {
                Port = Int32.Parse(_config["EmailSettings:Port"]),
                Credentials = new NetworkCredential(_config["EmailSettings:Username"], _config["EmailSettings:Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_config["EmailSettings:SenderEmail"], _config["EmailSettings.SenderName"]),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }

}
