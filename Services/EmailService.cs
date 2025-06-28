using FinDepen_Backend.Helper;
using MailKit.Security;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using FinDepen_Backend.Repositories;

namespace FinDepen_Backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings mailSettings;

        public EmailService(IOptions<MailSettings> options)
        {
            this.mailSettings = options.Value;

        }
        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(mailSettings.Email);
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            email.Subject = mailRequest.Subject;
            var builder = new BodyBuilder();
            builder.HtmlBody = mailRequest.Body;
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Connect(mailSettings.Host, mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(mailSettings.Email, mailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }

        public async Task SendPasswordResetOtpAsync(string toEmail, string otp)
        {
            if (string.IsNullOrWhiteSpace(mailSettings.Email))
            {
                throw new ArgumentNullException(nameof(mailSettings.Email), "Sender email cannot be null or empty.");
            }

            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(mailSettings.Email);
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Password Reset OTP";
            var builder = new BodyBuilder();
            builder.HtmlBody = $"<p>Your OTP for password reset is: <strong>{otp}</strong></p>";
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Connect(mailSettings.Host, mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(mailSettings.Email, mailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
    }
}
