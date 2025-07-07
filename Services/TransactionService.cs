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

        public async Task<IEnumerable<TransactionModel>> GetTransactions(string userId)
        {
            try
            {
                var transactions = await _context.Transactions
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.Date)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<TransactionModel>>(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving transactions for user: {UserId}", userId);
                throw new Exception("An error occurred while retrieving transactions.", ex);
            }
        }

        public async Task<TransactionModel> GetTransactionById(Guid id)
        {
            try
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id);
                
                if (transaction == null)
                {
                    throw new KeyNotFoundException("Transaction not found");
                }
                
                return _mapper.Map<TransactionModel>(transaction);
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

        public async Task<TransactionModel> CreateTransaction(CreateTransactionModel createModel, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create the transaction
                var transactionEntity = new Transaction
                {
                    Id = Guid.NewGuid(),
                    Title = createModel.Title,
                    Description = createModel.Description,
                    Amount = createModel.Amount,
                    Category = createModel.Category ?? "Income", // Use "Income" as default category for income transactions
                    Type = createModel.Type,
                    Date = createModel.Date,
                    UserId = userId
                };

                _context.Transactions.Add(transactionEntity);

                // Update user balance
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                // Initialize balance if null
                if (user.BalanceAmount == null)
                {
                    user.BalanceAmount = 0;
                }

                // Update balance based on transaction type
                if (createModel.Type == "Income")
                {
                    user.BalanceAmount += createModel.Amount;
                }
                else if (createModel.Type == "Expense")
                {
                    user.BalanceAmount -= createModel.Amount;
                }

                // Update budget spent amount if this is an expense and there's a matching budget
                if (createModel.Type == "Expense" && !string.IsNullOrEmpty(createModel.Category))
                {
                    var matchingBudget = await _context.Budgets
                        .FirstOrDefaultAsync(b => b.UserId == userId && 
                                                b.Category == createModel.Category &&
                                                b.StartDate <= createModel.Date &&
                                                (b.EndDate == null || b.EndDate >= createModel.Date));

                    if (matchingBudget != null)
                    {
                        matchingBudget.SpentAmount += createModel.Amount;
                        transactionEntity.BudgetId = matchingBudget.Id;
                        _context.Budgets.Update(matchingBudget);
                        _logger.LogInformation("Transaction linked to budget. Budget ID: {BudgetId}, Category: {Category}, Amount: {Amount}", 
                            matchingBudget.Id, createModel.Category, createModel.Amount);
                    }
                    else
                    {
                        _logger.LogInformation("No matching budget found for expense transaction. Category: {Category}, Date: {Date}", 
                            createModel.Category, createModel.Date);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Transaction created successfully. ID: {TransactionId}, User: {UserId}, Type: {Type}, Amount: {Amount}", 
                    transactionEntity.Id, userId, createModel.Type, createModel.Amount);

                return _mapper.Map<TransactionModel>(transactionEntity);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while creating transaction for user: {UserId}", userId);
                throw new Exception("An error occurred while creating the transaction.", ex);
            }
        }

        public async Task<TransactionModel> UpdateTransaction(Guid id, UpdateTransactionModel updateModel, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transactionToUpdate = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (transactionToUpdate == null)
                {
                    throw new KeyNotFoundException("Transaction not found or access denied");
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                // Calculate balance adjustment
                double balanceAdjustment = 0;
                
                // Remove the old transaction's impact on balance
                if (transactionToUpdate.Type == "Income")
                {
                    balanceAdjustment -= transactionToUpdate.Amount;
                }
                else if (transactionToUpdate.Type == "Expense")
                {
                    balanceAdjustment += transactionToUpdate.Amount;
                }

                // Add the new transaction's impact on balance
                if (updateModel.Type == "Income")
                {
                    balanceAdjustment += updateModel.Amount;
                }
                else if (updateModel.Type == "Expense")
                {
                    balanceAdjustment -= updateModel.Amount;
                }

                // Update user balance
                user.BalanceAmount += balanceAdjustment;

                // Update budget spent amounts
                await UpdateBudgetSpentAmount(transactionToUpdate, updateModel, userId);

                // Update transaction
                transactionToUpdate.Title = updateModel.Title;
                transactionToUpdate.Description = updateModel.Description;
                transactionToUpdate.Amount = updateModel.Amount;
                transactionToUpdate.Category = updateModel.Category;
                transactionToUpdate.Type = updateModel.Type;
                transactionToUpdate.Date = updateModel.Date;

                _context.Transactions.Update(transactionToUpdate);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Transaction updated successfully. ID: {TransactionId}, User: {UserId}", id, userId);

                return _mapper.Map<TransactionModel>(transactionToUpdate);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while updating transaction with id: {Id}", id);
                throw new Exception("An error occurred while updating the transaction.", ex);
            }
        }

        public async Task<TransactionModel> DeleteTransaction(Guid id, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transactionToDelete = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

                if (transactionToDelete == null)
                {
                    throw new KeyNotFoundException("Transaction not found or access denied");
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                // Update user balance
                if (transactionToDelete.Type == "Income")
                {
                    user.BalanceAmount -= transactionToDelete.Amount;
                }
                else if (transactionToDelete.Type == "Expense")
                {
                    user.BalanceAmount += transactionToDelete.Amount;
                }

                // Update budget spent amount if this transaction affected a budget
                if (transactionToDelete.BudgetId.HasValue)
                {
                    var budget = await _context.Budgets.FindAsync(transactionToDelete.BudgetId.Value);
                    if (budget != null)
                    {
                        budget.SpentAmount -= transactionToDelete.Amount;
                        _context.Budgets.Update(budget);
                        _logger.LogInformation("Removed transaction impact from budget on deletion. Budget ID: {BudgetId}, Amount: {Amount}", 
                            budget.Id, transactionToDelete.Amount);
                    }
                }

                _context.Transactions.Remove(transactionToDelete);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Transaction deleted successfully. ID: {TransactionId}, User: {UserId}", id, userId);

                return _mapper.Map<TransactionModel>(transactionToDelete);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while deleting transaction with id: {Id}", id);
                throw new Exception("An error occurred while deleting the transaction.", ex);
            }
        }

        public async Task<TransactionSummaryModel> GetTransactionSummary(string userId)
        {
            try
            {
                var currentMonth = DateTime.UtcNow;
                var startOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var transactions = await _context.Transactions
                    .Where(t => t.UserId == userId && t.Date >= startOfMonth && t.Date <= endOfMonth)
                    .ToListAsync();

                var summary = new TransactionSummaryModel
                {
                    TotalTransactions = transactions.Count,
                    TotalIncome = transactions.Where(t => t.Type == "Income").Sum(t => t.Amount),
                    TotalExpenses = transactions.Where(t => t.Type == "Expense").Sum(t => t.Amount),
                    RecentTransactions = _mapper.Map<List<TransactionModel>>(transactions.OrderByDescending(t => t.Date).Take(10))
                };

                summary.NetAmount = summary.TotalIncome - summary.TotalExpenses;

                // Category breakdown
                summary.CategoryBreakdown = transactions
                    .GroupBy(t => t.Category)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

                // Type breakdown
                summary.TypeBreakdown = transactions
                    .GroupBy(t => t.Type)
                    .ToDictionary(g => g.Key, g => g.Count());

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting transaction summary for user: {UserId}", userId);
                throw new Exception("An error occurred while getting transaction summary.", ex);
            }
        }

        private async Task UpdateBudgetSpentAmount(Transaction oldTransaction, UpdateTransactionModel newTransaction, string userId)
        {
            // Remove old transaction's impact on budget
            if (oldTransaction.BudgetId.HasValue)
            {
                var oldBudget = await _context.Budgets.FindAsync(oldTransaction.BudgetId.Value);
                if (oldBudget != null)
                {
                    oldBudget.SpentAmount -= oldTransaction.Amount;
                    _context.Budgets.Update(oldBudget);
                    _logger.LogInformation("Removed transaction impact from budget. Budget ID: {BudgetId}, Amount: {Amount}", 
                        oldBudget.Id, oldTransaction.Amount);
                }
            }

            // Add new transaction's impact on budget if it's an expense
            if (newTransaction.Type == "Expense" && !string.IsNullOrEmpty(newTransaction.Category))
            {
                var matchingBudget = await _context.Budgets
                    .FirstOrDefaultAsync(b => b.UserId == userId && 
                                            b.Category == newTransaction.Category &&
                                            b.StartDate <= newTransaction.Date &&
                                            (b.EndDate == null || b.EndDate >= newTransaction.Date));

                if (matchingBudget != null)
                {
                    matchingBudget.SpentAmount += newTransaction.Amount;
                    oldTransaction.BudgetId = matchingBudget.Id; // Update the BudgetId on the transaction
                    _context.Budgets.Update(matchingBudget);
                    _logger.LogInformation("Added transaction impact to budget. Budget ID: {BudgetId}, Amount: {Amount}", 
                        matchingBudget.Id, newTransaction.Amount);
                }
                else
                {
                    // Clear BudgetId if no matching budget found
                    oldTransaction.BudgetId = null;
                    _logger.LogInformation("No matching budget found for transaction category: {Category}", newTransaction.Category);
                }
            }
            else
            {
                // Clear BudgetId for non-expense transactions
                oldTransaction.BudgetId = null;
            }
        }
    }
}
