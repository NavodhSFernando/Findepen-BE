namespace FinDepen_Backend.Services
{
    public interface IRecurringTransactionProcessingService
    {
        Task ProcessRecurringTransactions();
        Task ProcessRecurringTransaction(Guid recurringTransactionId);
    }
}
