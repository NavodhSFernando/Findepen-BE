using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinDepen_Backend.Entities
{
    public class DailyReserveSnapshot
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public double ReserveAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
