using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinDepen_Backend.Constants;

namespace FinDepen_Backend.DTOs
{
    public class GoalModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 100 characters")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Target amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Target amount must be greater than 0")]
        public double TargetAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Current amount cannot be negative")]
        public double CurrentAmount { get; set; }

        [Required(ErrorMessage = "Target date is required")]
        public DateTime TargetDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastUpdatedDate { get; set; }

        public GoalPriority Priority { get; set; } = GoalPriority.Medium;

        public bool IsActive { get; set; } = true;

        public bool Reminder { get; set; } = false;

        public GoalStatus Status { get; set; } = GoalStatus.Active;

        public string UserId { get; set; }

        // Calculated properties
        [NotMapped]
        public double ProgressPercentage => TargetAmount > 0 ? Math.Min((CurrentAmount / TargetAmount) * 100, 100) : 0;

        [NotMapped]
        public double RemainingAmount => Math.Max(TargetAmount - CurrentAmount, 0);

        [NotMapped]
        public int DaysRemaining => Math.Max((TargetDate - DateTime.UtcNow).Days, 0);

        [NotMapped]
        public bool IsOverdue => DateTime.UtcNow > TargetDate && CurrentAmount < TargetAmount;

        [NotMapped]
        public bool IsCompleted => CurrentAmount >= TargetAmount;

        [NotMapped]
        public string ProgressStatus
        {
            get
            {
                if (IsCompleted)
                    return "Completed";
                else if (IsOverdue)
                    return "Overdue";
                else if (ProgressPercentage >= 80)
                    return "Near Completion";
                else if (ProgressPercentage >= 50)
                    return "Good Progress";
                else if (ProgressPercentage >= 25)
                    return "In Progress";
                else
                    return "Just Started";
            }
        }

        [NotMapped]
        public double MonthlyRequiredAmount
        {
            get
            {
                var monthsRemaining = Math.Max((TargetDate - DateTime.UtcNow).Days / 30.0, 1);
                return Math.Max(RemainingAmount / monthsRemaining, 0);
            }
        }

        [NotMapped]
        public double WeeklyRequiredAmount
        {
            get
            {
                var weeksRemaining = Math.Max((TargetDate - DateTime.UtcNow).Days / 7.0, 1);
                return Math.Max(RemainingAmount / weeksRemaining, 0);
            }
        }
    }

    public class CreateGoalModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 100 characters")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Target amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Target amount must be greater than 0")]
        public double TargetAmount { get; set; }

        [Required(ErrorMessage = "Target date is required")]
        public DateTime TargetDate { get; set; } = DateTime.UtcNow.AddMonths(1);

        public GoalPriority Priority { get; set; } = GoalPriority.Medium;

        public bool Reminder { get; set; } = false;
    }

    public class UpdateGoalModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 100 characters")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Target amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Target amount must be greater than 0")]
        public double TargetAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Current amount cannot be negative")]
        public double CurrentAmount { get; set; }

        [Required(ErrorMessage = "Target date is required")]
        public DateTime TargetDate { get; set; }

        public GoalPriority Priority { get; set; }

        public bool IsActive { get; set; }

        public bool Reminder { get; set; }

        public GoalStatus Status { get; set; }
    }

    public class AddFundsToGoalModel
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public double Amount { get; set; }

        [StringLength(200, ErrorMessage = "Note cannot exceed 200 characters")]
        public string? Note { get; set; }
    }

    public class ConvertGoalToExpenseModel
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Transaction title is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Transaction title must be between 2 and 100 characters")]
        public string TransactionTitle { get; set; }

        [StringLength(500, ErrorMessage = "Transaction description cannot exceed 500 characters")]
        public string? TransactionDescription { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }

        [Required(ErrorMessage = "Goal completion status is required")]
        public bool MarkGoalAsCompleted { get; set; }

        [StringLength(200, ErrorMessage = "Note cannot exceed 200 characters")]
        public string? Note { get; set; }
    }

    public class GoalSummaryModel
    {
        public int TotalGoals { get; set; }
        public int ActiveGoals { get; set; }
        public int CompletedGoals { get; set; }
        public int OverdueGoals { get; set; }
        public double TotalTargetAmount { get; set; }
        public double TotalCurrentAmount { get; set; }
        public double TotalRemainingAmount { get; set; }
        public double OverallProgressPercentage { get; set; }
        public double TotalMonthlyRequired { get; set; }
        public double TotalWeeklyRequired { get; set; }
        public Dictionary<GoalPriority, int> PriorityBreakdown { get; set; } = new();
        public Dictionary<GoalStatus, int> StatusBreakdown { get; set; } = new();
    }
} 