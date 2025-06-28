namespace FinDepen_Backend.Entities
{
    public class RecurringTransaction : Transaction
    {
        public string Frequency { get; set; }
        public DateTime StartDate { get; set; }
    }
}
