﻿using System.ComponentModel.DataAnnotations;

namespace FinDepen_Backend.DTOs
{
    public class VerifyOtpModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must contain exactly 6 digits")]
        public string Otp { get; set; } = string.Empty;
    }
}
