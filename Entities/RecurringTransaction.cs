using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using FinDepen_Backend.Constants;
using FinDepen_Backend.Validation;

namespace FinDepen_Backend.Entities
{
    public class RecurringTransaction : Transaction
    {
        [Required(ErrorMessage = "Frequency is required")]
        [ValidRenewalFrequency]
        public RenewalFrequency Frequency { get; set; } = RenewalFrequency.Monthly;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Next occurrence date is required")]
        public DateTime NextOccurrenceDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [ValidRecurringTransactionStatus]
        public RecurringTransactionStatus Status { get; set; } = RecurringTransactionStatus.Active;

        public int OccurrenceCount { get; set; } = 0;

        public DateTime? LastCreatedDate { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        // Calculated properties
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

        // Navigation property for generated transactions
        [JsonIgnore]
        public ICollection<Transaction> GeneratedTransactions { get; set; } = new List<Transaction>();
    }
}
