using System.ComponentModel.DataAnnotations;

namespace FinDepen_Backend.DTOs
{
    // DTO for updating user profile
    public class UpdateProfileModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }
    }

    // DTO for user profile responses
    public class UserProfileModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DOB { get; set; }
        public double? BalanceAmount { get; set; }
        public string Theme { get; set; } = "light";
        // Calculated properties
        public int Age => DateTime.UtcNow.Year - DOB.Year - (DateTime.UtcNow.DayOfYear < DOB.DayOfYear ? 1 : 0);
        public string FormattedDOB => DOB.ToString("MMM dd, yyyy");
        public string FormattedBalance => BalanceAmount?.ToString("C") ?? "$0.00";
    }

    // DTO for changing password
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    // DTO for balance adjustments
    public class BalanceAdjustmentModel
    {
        [Required(ErrorMessage = "Adjustment amount is required")]
        [Range(-1000000, 1000000, ErrorMessage = "Adjustment amount must be between -1,000,000 and 1,000,000")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Reason for adjustment is required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Reason must be between 5 and 500 characters")]
        public string Reason { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Additional notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
    }

    // DTO for user settings
    public class UserSettingsModel
    {
        [Required(ErrorMessage = "Theme is required")]
        [RegularExpression("^(light|dark)$", ErrorMessage = "Theme must be either 'light' or 'dark'")]
        public string Theme { get; set; } = "light";
    }
} 