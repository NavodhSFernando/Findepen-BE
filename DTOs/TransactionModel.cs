using System.ComponentModel.DataAnnotations;
using FinDepen_Backend.Validation;

namespace FinDepen_Backend.DTOs
{
    public class TransactionModel
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public double Amount { get; set; }
        
        [ValidCategory]
        public string Category { get; set; }
        
        public string Type { get; set; }
        public DateTime Date { get; set; }
    }
}
