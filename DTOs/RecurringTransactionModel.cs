using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinDepen_Backend.Constants;
using FinDepen_Backend.Validation;

namespace FinDepen_Backend.DTOs
{
    // Main DTO for recurring transaction responses with calculated properties
    public class RecurringTransactionModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 100 characters")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [ValidCategory]
        public string Category { get; set; }

        [Required(ErrorMessage = "Transaction type is required")]
        [RegularExpression("^(Income|Expense)$", ErrorMessage = "Transaction type must be either 'Income' or 'Expense'")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        public string UserId { get; set; }

        public Guid? BudgetId { get; set; }

        // Recurring-specific properties
        [Required(ErrorMessage = "Frequency is required")]
        [ValidRenewalFrequency]
        public RenewalFrequency Frequency { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Next occurrence date is required")]
        public DateTime NextOccurrenceDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [ValidRecurringTransactionStatus]
        public RecurringTransactionStatus Status { get; set; }

        public int OccurrenceCount { get; set; }

        public DateTime? LastCreatedDate { get; set; }

        // Calculated properties from base Transaction
        [NotMapped]
        public double FormattedAmount => Math.Round(Amount, 2);

        [NotMapped]
        public string FormattedDate => Date.ToString("MMM dd, yyyy");

        [NotMapped]
        public string BalanceImpact => Type == "Income" ? "+" : "-";

        [NotMapped]
        public string FormattedAmountWithSign => $"{BalanceImpact}{FormattedAmount:C}";

        [NotMapped]
        public bool IsIncome => Type == "Income";

        [NotMapped]
        public bool IsExpense => Type == "Expense";

        // Calculated properties for recurring functionality
        [NotMapped]
        public bool IsActive => Status == RecurringTransactionStatus.Active;

        [NotMapped]
        public bool CanBeProcessed => Status == RecurringTransactionStatus.Active && 
                                    NextOccurrenceDate <= DateTime.UtcNow &&
                                    (EndDate == null || EndDate > DateTime.UtcNow);

        [NotMapped]
        public bool IsExpired => EndDate.HasValue && EndDate.Value <= DateTime.UtcNow;

        [NotMapped]
        public int DaysUntilNextOccurrence => (NextOccurrenceDate - DateTime.UtcNow).Days;

        [NotMapped]
        public string StatusDisplayName => Status.GetDisplayName();

        [NotMapped]
        public string FrequencyDisplayName => Frequency.GetDisplayName();

        [NotMapped]
        public string NextOccurrenceFormatted => NextOccurrenceDate.ToString("MMM dd, yyyy");

        [NotMapped]
        public string StartDateFormatted => StartDate.ToString("MMM dd, yyyy");

        [NotMapped]
        public string? EndDateFormatted => EndDate?.ToString("MMM dd, yyyy");
    }

    // DTO for creating new recurring transactions
    public class CreateRecurringTransactionModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 100 characters")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public double Amount { get; set; }

        [ValidCategory]
        public string? Category { get; set; }

        [Required(ErrorMessage = "Transaction type is required")]
        [RegularExpression("^(Income|Expense)$", ErrorMessage = "Transaction type must be either 'Income' or 'Expense'")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Frequency is required")]
        [ValidRenewalFrequency]
        public RenewalFrequency Frequency { get; set; } = RenewalFrequency.Monthly;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }
    }

    // DTO for updating existing recurring transactions
    public class UpdateRecurringTransactionModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 100 characters")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public double Amount { get; set; }

        [ValidCategory]
        public string? Category { get; set; }

        [Required(ErrorMessage = "Transaction type is required")]
        [RegularExpression("^(Income|Expense)$", ErrorMessage = "Transaction type must be either 'Income' or 'Expense'")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Frequency is required")]
        [ValidRenewalFrequency]
        public RenewalFrequency Frequency { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [ValidRecurringTransactionStatus]
        public RecurringTransactionStatus Status { get; set; }
    }

    // DTO for updating recurring transaction status
    public class UpdateRecurringTransactionStatusModel
    {
        [Required(ErrorMessage = "Status is required")]
        [ValidRecurringTransactionStatus]
        public RecurringTransactionStatus Status { get; set; }
    }

    // DTO for recurring transaction summary and analytics
    public class RecurringTransactionSummaryModel
    {
        public int TotalRecurringTransactions { get; set; }
        public int ActiveRecurringTransactions { get; set; }
        public int PausedRecurringTransactions { get; set; }
        public int CancelledRecurringTransactions { get; set; }
        public double TotalMonthlyAmount { get; set; }
        public double TotalWeeklyAmount { get; set; }
        public double TotalYearlyAmount { get; set; }
        public Dictionary<string, double> CategoryBreakdown { get; set; } = new();
        public Dictionary<string, int> TypeBreakdown { get; set; } = new();
        public List<RecurringTransactionModel> RecentRecurringTransactions { get; set; } = new();
        
        // Calculated properties
        [NotMapped]
        public double FormattedTotalMonthlyAmount => Math.Round(TotalMonthlyAmount, 2);

        [NotMapped]
        public double FormattedTotalWeeklyAmount => Math.Round(TotalWeeklyAmount, 2);

        [NotMapped]
        public double FormattedTotalYearlyAmount => Math.Round(TotalYearlyAmount, 2);

        [NotMapped]
        public double TotalActiveAmount => TotalMonthlyAmount + TotalWeeklyAmount + TotalYearlyAmount;

        [NotMapped]
        public double FormattedTotalActiveAmount => Math.Round(TotalActiveAmount, 2);
    }
}
