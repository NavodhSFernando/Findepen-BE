using AutoMapper;
using FinDepen_Backend.Data;
using FinDepen_Backend.DTOs;
using FinDepen_Backend.Entities;
using FinDepen_Backend.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinDepen_Backend.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TransactionService> _logger;
        public TransactionService(ApplicationDbContext context, IMapper mapper, ILogger<TransactionService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(string userId)
        {
            try
            {
                return await _context.Transactions
                    .Where(t => t.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving transactions for user: {UserId}", userId);
                throw new Exception("An error occurred while retrieving transactions.", ex);
            }
        }

        public async Task<Transaction> GetTransactionById(Guid id)
        {
            try
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id);
                if (transaction == null)
                {
                    throw new KeyNotFoundException("Transaction not found");
                }
                return transaction;
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Transaction not found with id: {Id}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the transaction with id: {Id}", id);
                throw new Exception("An error occurred while retrieving the transaction.", ex);
            }
        }

        public async Task<Transaction> CreateTransaction(Transaction transaction)
        {
            try
            {
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                return transaction;
            }
            catch (DbUpdateException ex)
            {
                // Handle database update exception
                throw new Exception("An error occurred while saving the transaction.", ex);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                throw new Exception("An unexpected error occurred.", ex);
            }
        }

        public async Task<Transaction> UpdateTransaction(Guid id, TransactionModel transaction)
        {
            try
            {
                var transactionToUpdate = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id);
                if (transactionToUpdate == null)
                {
                    throw new Exception("Transaction not found");
                }

                if (transaction != null)
                {
                    transactionToUpdate.Title = transaction.Title;
                }
                if (transaction.Description != null)
                {
                    transactionToUpdate.Description = transaction.Description;
                }
                if (transaction.Amount != 0)
                {
                    transactionToUpdate.Amount = transaction.Amount;
                }
                if (transaction.Category != null)
                {
                    transactionToUpdate.Category = transaction.Category;
                }
                if (transaction.Type != null)
                {
                    transactionToUpdate.Type = transaction.Type;
                }
                if (transaction.Date != default)
                {
                    transactionToUpdate.Date = transaction.Date;
                }

                _context.Transactions.Update(transactionToUpdate);
                await _context.SaveChangesAsync();
                return transactionToUpdate;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while updating the transaction with id: {Id}", id);
                throw new Exception("An error occurred while updating the transaction.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating the transaction with id: {Id}", id);
                throw new Exception("An unexpected error occurred.", ex);
            }
        }

        public async Task<Transaction> DeleteTransaction(Guid id)
        {
            try
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (transaction == null)
                {
                    throw new Exception("Transaction not found");
                }

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();

                return transaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the transaction with id: {Id}", id);
                throw new Exception("An error occurred while deleting the transaction.", ex);
            }
        }






    }
}
