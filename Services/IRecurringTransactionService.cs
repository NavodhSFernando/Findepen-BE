using FinDepen_Backend.DTOs;

namespace FinDepen_Backend.Services
{
    public interface IRecurringTransactionService
    {
        Task<IEnumerable<RecurringTransactionModel>> GetRecurringTransactions(string userId);
        Task<RecurringTransactionModel> GetRecurringTransactionById(Guid id, string userId);
        Task<RecurringTransactionModel> CreateRecurringTransaction(CreateRecurringTransactionModel createModel, string userId);
        Task<RecurringTransactionModel> UpdateRecurringTransaction(Guid id, UpdateRecurringTransactionModel updateModel, string userId);
        Task<RecurringTransactionModel> UpdateRecurringTransactionStatus(Guid id, UpdateRecurringTransactionStatusModel statusModel, string userId);
        Task<bool> DeleteRecurringTransaction(Guid id, string userId);
        Task<RecurringTransactionSummaryModel> GetRecurringTransactionSummary(string userId);
        Task<IEnumerable<RecurringTransactionModel>> GetRecurringTransactionsByStatus(string userId, string status);
        Task<IEnumerable<RecurringTransactionModel>> GetRecurringTransactionsByFrequency(string userId, string frequency);
        Task<bool> PauseRecurringTransaction(Guid id, string userId);
        Task<bool> ResumeRecurringTransaction(Guid id, string userId);
        Task<bool> CancelRecurringTransaction(Guid id, string userId);
        Task<DateTime> CalculateNextOccurrenceDate(DateTime currentDate, string frequency);
        Task<IEnumerable<RecurringTransactionModel>> GetRecurringTransactionsReadyForProcessing();
    }
}
