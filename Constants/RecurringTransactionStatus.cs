namespace FinDepen_Backend.Constants
{
    public enum RecurringTransactionStatus
    {
        Active,
        Paused,
        Cancelled
    }

    public static class RecurringTransactionStatusExtensions
    {
        public static string GetDisplayName(this RecurringTransactionStatus status)
        {
            return status switch
            {
                RecurringTransactionStatus.Active => "Active",
                RecurringTransactionStatus.Paused => "Paused",
                RecurringTransactionStatus.Cancelled => "Cancelled",
                _ => "Active"
            };
        }

        public static bool IsActive(this RecurringTransactionStatus status)
        {
            return status == RecurringTransactionStatus.Active;
        }

        public static bool CanBeProcessed(this RecurringTransactionStatus status)
        {
            return status == RecurringTransactionStatus.Active;
        }
    }
}
