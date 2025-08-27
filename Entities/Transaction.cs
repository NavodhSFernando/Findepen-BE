using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FinDepen_Backend.Entities
{
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }
        public string? Description { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        [JsonIgnore]
        public string UserId { get; set; }

        [JsonIgnore]
        public ApplicationUser User { get; set; }

        // Navigation property for related budget (if this transaction affects a budget)
        public Guid? BudgetId { get; set; }
        
        [JsonIgnore]
        public Budget? Budget { get; set; }

        // Recurring transaction tracking
        public bool IsRecurringGenerated { get; set; } = false;
        public Guid? RecurringTransactionId { get; set; }
        
        [JsonIgnore]
        public RecurringTransaction? RecurringTransaction { get; set; }
    }
}
