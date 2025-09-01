using FinDepen_Backend.DTOs;
using FinDepen_Backend.Entities;

namespace FinDepen_Backend.Repositories
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionModel>> GetTransactions(string userId);
        Task<TransactionModel> GetTransactionById(Guid id);
        Task<TransactionModel> CreateTransaction(CreateTransactionModel createModel, string userId);
        Task<TransactionModel> UpdateTransaction(Guid id, UpdateTransactionModel updateModel, string userId);
        Task<TransactionModel> DeleteTransaction(Guid id, string userId);
        Task<TransactionSummaryModel> GetTransactionSummary(string userId);
    }
}
