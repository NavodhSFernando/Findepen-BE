namespace FinDepen_Backend.DTOs
{
    public class ResetPasswordModel
    {
        public string Email { get; set; }
        public string Otp { get; set; }
        public string NewPassword { get; set; }
    }
}
