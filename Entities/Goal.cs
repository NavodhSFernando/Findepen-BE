using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using FinDepen_Backend.Constants;

namespace FinDepen_Backend.Entities
{
    public class Goal
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }

        [Required]
        public double TargetAmount { get; set; }
        public double CurrentAmount { get; set; } = 0;

        [Required]
        public DateTime TargetDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedDate { get; set; }
        public GoalPriority Priority { get; set; } = GoalPriority.Medium;
        public bool IsActive { get; set; } = true;
        public bool Reminder { get; set; } = false;
        public GoalStatus Status { get; set; } = GoalStatus.Active;

        [ForeignKey("UserId")]
        [JsonIgnore]
        public string UserId { get; set; }

        [JsonIgnore]
        public ApplicationUser User { get; set; }
    }
}
