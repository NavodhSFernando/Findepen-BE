using FinDepen_Backend.Entities;
using Microsoft.AspNetCore.Identity;

namespace FinDepen_Backend.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public DateTime DOB { get; set; }
        public double? BalanceAmount { get; set; } = -1.0;
        public string? PasswordResetOtp { get; set; }
        public DateTime? PasswordResetOtpExpiry { get; set; }
        public string Theme { get; set; } = "light";
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
        public ICollection<Goal> Goals { get; set; } = new List<Goal>();

    }
}