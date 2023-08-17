using System;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace SsoServer.Infrastructures
{
    public class EmailSender : IEmailSender
    {

        // Our private configuration variables
        private string host;
        private int port;
        private bool enableSSL;
        private string userName;
        private string password;

        // Get our parameterized configuration
        public EmailSender(string host, int port, bool enableSSL, string userName, string password)
        {
            this.host = host;
            this.port = port;
            this.enableSSL = enableSSL;

            this.userName = userName;
            this.password = password;
        }

        // Use our configuration to send the email by using SmtpClient
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = enableSSL
            };
            var emails = email.Split(";");

            var mailMessage = new MailMessage(userName, emails[0], subject, htmlMessage) { IsBodyHtml = true };
            foreach (var e in emails.Skip(1))
            {
                if (e.Contains("@"))
                    mailMessage.To.Add(e);
            }
            return client.SendMailAsync(
               mailMessage
            );
        }
    }

}

