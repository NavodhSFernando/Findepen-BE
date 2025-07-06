using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using FinDepen_Backend.Constants;

namespace FinDepen_Backend.Entities
{
    public class Budget
    {
        [Key]
        public Guid Id { get; set; }
        public string Category { get; set; }
        public double PlannedAmount { get; set; }
        public double SpentAmount { get; set; }

        public bool Reminder { get; set; }

        [Required]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [Required]
        public RenewalFrequency RenewalFrequency { get; set; } = RenewalFrequency.Monthly; // Monthly, Weekly, Yearly

        // Auto-renewal fields
        public bool AutoRenewalEnabled { get; set; } = false;
        public DateTime? LastRenewalDate { get; set; }
        public int RenewalCount { get; set; } = 0;
        public DateTime? EndDate { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public string UserId { get; set; }

        [JsonIgnore]
        public ApplicationUser User { get; set; }
    }
}
