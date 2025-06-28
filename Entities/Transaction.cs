using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FinDepen_Backend.Entities
{
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public double Amount { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public string UserId { get; set; }

        [JsonIgnore]
        public ApplicationUser User { get; set; }
    }
}
