using FinDepen_Backend.Helper;

namespace FinDepen_Backend.Repositories
{
    public interface IEmailService
    {
        Task SendEmailAsync(MailRequest mailRequest);
        Task SendPasswordResetOtpAsync(string toEmail, string otp);
    }
}
