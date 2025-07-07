using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinDepen_Backend.Validation;
using FinDepen_Backend.Constants;

namespace FinDepen_Backend.DTOs
{
    // Main DTO for transaction responses with calculated properties
    public class TransactionModel
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
        public DateTime Date { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; }

        public Guid? BudgetId { get; set; }

        // Calculated properties
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
    }

    // DTO for creating new transactions
    public class CreateTransactionModel
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
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }

    // DTO for updating existing transactions
    public class UpdateTransactionModel
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
    }

    // DTO for transaction summary and analytics
    public class TransactionSummaryModel
    {
        public int TotalTransactions { get; set; }
        public double TotalIncome { get; set; }
        public double TotalExpenses { get; set; }
        public double NetAmount { get; set; }
        public Dictionary<string, double> CategoryBreakdown { get; set; } = new();
        public Dictionary<string, int> TypeBreakdown { get; set; } = new();
        public List<TransactionModel> RecentTransactions { get; set; } = new();
        
        // Calculated properties
        [NotMapped]
        public double FormattedTotalIncome => Math.Round(TotalIncome, 2);

        [NotMapped]
        public double FormattedTotalExpenses => Math.Round(TotalExpenses, 2);

        [NotMapped]
        public double FormattedNetAmount => Math.Round(NetAmount, 2);

        [NotMapped]
        public string NetAmountFormatted => NetAmount >= 0 ? $"+{FormattedNetAmount:C}" : $"{FormattedNetAmount:C}";
    }

    // DTO for user balance management
    public class UserBalanceModel
    {
        public double CurrentBalance { get; set; }
        public double MonthlyIncome { get; set; }
        public double MonthlyExpenses { get; set; }
        public double MonthlyNet { get; set; }
        
        // Calculated properties
        [NotMapped]
        public double FormattedCurrentBalance => Math.Round(CurrentBalance, 2);

        [NotMapped]
        public double FormattedMonthlyIncome => Math.Round(MonthlyIncome, 2);

        [NotMapped]
        public double FormattedMonthlyExpenses => Math.Round(MonthlyExpenses, 2);

        [NotMapped]
        public double FormattedMonthlyNet => Math.Round(MonthlyNet, 2);

        [NotMapped]
        public string MonthlyNetFormatted => MonthlyNet >= 0 ? $"+{FormattedMonthlyNet:C}" : $"{FormattedMonthlyNet:C}";
    }

    // DTO for setting initial balance
    public class SetInitialBalanceModel
    {
        [Required(ErrorMessage = "Initial balance is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Initial balance cannot be negative")]
        public double InitialBalance { get; set; }
    }
}
