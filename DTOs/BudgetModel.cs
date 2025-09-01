using System.ComponentModel.DataAnnotations;
using FinDepen_Backend.Validation;
using FinDepen_Backend.Constants;

namespace FinDepen_Backend.DTOs
{
    public class BudgetModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [ValidCategory]
        public string Category { get; set; }

        [Required(ErrorMessage = "Planned amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Planned amount must be greater than 0")]
        public double PlannedAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Spent amount cannot be negative")]
        public double SpentAmount { get; set; }

        public bool Reminder { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Renewal frequency is required")]
        [ValidRenewalFrequency]
        public RenewalFrequency RenewalFrequency { get; set; } = RenewalFrequency.Monthly;


        public string UserId { get; set; }

        // Calculated property for remaining budget
        public double RemainingAmount => PlannedAmount - SpentAmount;

        // Calculated property for budget progress percentage
        public double ProgressPercentage => PlannedAmount > 0 ? (SpentAmount / PlannedAmount) * 100 : 0;

        // Status property based on spending
        public string Status
        {
            get
            {
                if (SpentAmount >= PlannedAmount)
                    return "Exceeded";
                else if (SpentAmount >= PlannedAmount * 0.8)
                    return "Warning";
                else
                    return "On Track";
            }
        }

        // Calculated property for days remaining in current period
        public int DaysRemainingInPeriod
        {
            get
            {
                var nextRenewal = StartDate.AddMonths(1); // Temporary fallback
                var daysRemaining = (nextRenewal - DateTime.UtcNow).Days;
                return Math.Max(0, daysRemaining);
            }
        }

        // Calculated property for next renewal date
        public DateTime NextRenewalDate
        {
            get
            {
                return RenewalFrequency switch
                {
                    Constants.RenewalFrequency.Weekly => StartDate.AddDays(7),
                    Constants.RenewalFrequency.Monthly => StartDate.AddMonths(1),
                    Constants.RenewalFrequency.Yearly => StartDate.AddYears(1),
                    _ => StartDate.AddMonths(1)
                };
            }
        }
    }

    public class CreateBudgetModel
    {
        [Required(ErrorMessage = "Category is required")]
        [ValidCategory]
        public string Category { get; set; }

        [Required(ErrorMessage = "Planned amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Planned amount must be greater than 0")]
        public double PlannedAmount { get; set; }

        public bool Reminder { get; set; } = false;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Renewal frequency is required")]
        [ValidRenewalFrequency]
        public RenewalFrequency RenewalFrequency { get; set; } = RenewalFrequency.Monthly;

        public bool AutoRenewalEnabled { get; set; } = false;
    }

    public class UpdateBudgetModel
    {
        [Required(ErrorMessage = "Category is required")]
        [ValidCategory]
        public string Category { get; set; }

        [Required(ErrorMessage = "Planned amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Planned amount must be greater than 0")]
        public double PlannedAmount { get; set; }

        public bool Reminder { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Renewal frequency is required")]
        [ValidRenewalFrequency]
        public RenewalFrequency RenewalFrequency { get; set; }

        public bool AutoRenewalEnabled { get; set; }
    }

    public class BudgetSummaryModel
    {
        public int TotalBudgets { get; set; }
        public double TotalPlannedAmount { get; set; }
        public double TotalSpentAmount { get; set; }
        public double TotalRemainingAmount { get; set; }
        public double OverallProgressPercentage { get; set; }
        public int OnTrackBudgets { get; set; }
        public int WarningBudgets { get; set; }
        public int ExceededBudgets { get; set; }
    }

    public class ToggleAutoRenewalModel
    {
        [Required(ErrorMessage = "Auto-renewal status is required")]
        public bool AutoRenewalEnabled { get; set; }
    }
} 
