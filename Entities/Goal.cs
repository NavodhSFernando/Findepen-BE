using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FinDepen_Backend.Entities
{
    public class Goal
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public double TargetAmount { get; set; }
        public double CurrentAmount { get; set; }
        public DateTime TargetDate { get; set; }
        public bool Reminder { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public string UserId { get; set; }

        [JsonIgnore]
        public ApplicationUser User { get; set; }
    }
}
