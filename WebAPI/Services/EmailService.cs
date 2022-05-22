using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebAPI.Services
{
    public class EmailService
    {
        public struct Attachment
        {
            public string FileName { get; set; }
            public Stream Content { get; set; }
        }

        private class MailSettings
        {
            public string Host { get; set; }
            public int Port { get; set; }
            public string Sender { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private readonly MailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _settings = configuration.GetSection(nameof(MailSettings)).Get<MailSettings>();
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody, IEnumerable<Attachment> attachments = null)
        {
            var message = new MimeMessage();
            message.Sender = MailboxAddress.Parse(_settings.Sender);
            message.From.Add(MailboxAddress.Parse(_settings.Sender));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = htmlBody;

            if (attachments != null)
                foreach (var attachment in attachments)
                {
                    string mimeType = MimeTypes.MimeTypeMap.GetMimeType(attachment.FileName);
                    var types = mimeType.Split('/');
                    var contentType = new ContentType(types[0], types[1]);
                    bodyBuilder.Attachments.Add(attachment.FileName, attachment.Content, contentType);
                }

            message.Body = bodyBuilder.ToMessageBody();

            using (var smtpClient = new SmtpClient())
            {
                await smtpClient.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_settings.Username, _settings.Password);
                smtpClient.Timeout = 120000;

                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true);
            }
        }
    }
}
