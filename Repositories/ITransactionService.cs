using FinDepen_Backend.DTOs;
using FinDepen_Backend.Entities;

namespace FinDepen_Backend.Repositories
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetTransactions(string userId);
        Task<Transaction> GetTransactionById(Guid id);
        Task<Transaction> CreateTransaction(Transaction transaction);
        Task<Transaction> UpdateTransaction(Guid id, TransactionModel transaction);
        Task<Transaction> DeleteTransaction(Guid id);
    }
}
