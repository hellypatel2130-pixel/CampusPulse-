using System.Net;
using System.Net.Mail;

namespace UserRoles.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            string? from = _configuration["EmailSettings:From"];
            string? smtpServer = _configuration["EmailSettings:SmtpServer"];
            string? username = _configuration["EmailSettings:Username"];
            string? password = _configuration["EmailSettings:Password"];

            int port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");

            if (string.IsNullOrEmpty(from) ||
                string.IsNullOrEmpty(smtpServer) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                throw new Exception("EmailSettings are missing in appsettings.json");
            }

            using var message = new MailMessage();
            message.From = new MailAddress(from);
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            await client.SendMailAsync(message);
        }
    }
}