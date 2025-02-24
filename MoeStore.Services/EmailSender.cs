using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MoeStore.Services
{
    public class EmailSender
    {
        private readonly string apiKey;
        private readonly string fromEmail;
        private readonly string senderName;
        public EmailSender(IConfiguration config)
        {
            apiKey = config["EmailSender:ApiKey"]!;
            fromEmail = config["EmailSender:FromEmail"]!;
            senderName = config["EmailSender:SenderName"]!;
        }

        public async Task SendEmail(string subject, string toEmail, string username, string message)
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, senderName);
            var to = new EmailAddress(toEmail, username);
            var plainTextContent = message;
            var htmlContent = "";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
