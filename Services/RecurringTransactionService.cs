using AutoMapper;
using FinDepen_Backend.Constants;
using FinDepen_Backend.Data;
using FinDepen_Backend.DTOs;
using FinDepen_Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinDepen_Backend.Services
{
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<RecurringTransactionService> _logger;

        public RecurringTransactionService(ApplicationDbContext context, IMapper mapper, ILogger<RecurringTransactionService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<RecurringTransactionModel>> GetRecurringTransactions(string userId)
        {
            try
            {
                var recurringTransactions = await _context.RecurringTransactions
                    .Where(rt => rt.UserId == userId)
                    .OrderByDescending(rt => rt.CreatedDate)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<RecurringTransactionModel>>(recurringTransactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving recurring transactions for user: {UserId}", userId);
                throw new Exception("An error occurred while retrieving recurring transactions.", ex);
            }
        }

        public async Task<RecurringTransactionModel> GetRecurringTransactionById(Guid id, string userId)
        {
            try
            {
                var recurringTransaction = await _context.RecurringTransactions
                    .FirstOrDefaultAsync(rt => rt.Id == id && rt.UserId == userId);
                
                if (recurringTransaction == null)
                {
                    throw new KeyNotFoundException("Recurring transaction not found");
                }
                
                return _mapper.Map<RecurringTransactionModel>(recurringTransaction);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recurring transaction not found with id: {Id} for user: {UserId}", id, userId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the recurring transaction with id: {Id}", id);
                throw new Exception("An error occurred while retrieving the recurring transaction.", ex);
            }
        }

        public async Task<RecurringTransactionModel> CreateRecurringTransaction(CreateRecurringTransactionModel createModel, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate the model
                ValidateCreateRecurringTransactionModel(createModel);

                // Create the recurring transaction
                var recurringTransaction = _mapper.Map<RecurringTransaction>(createModel);
                recurringTransaction.Id = Guid.NewGuid();
                recurringTransaction.UserId = userId;
                recurringTransaction.Category = createModel.Category ?? "Income"; // Use "Income" as default category for income transactions
                // Calculate the next occurrence date based on the start date and frequency
                recurringTransaction.NextOccurrenceDate = CalculateNextOccurrenceDate(createModel.StartDate, createModel.Frequency);
                recurringTransaction.Status = RecurringTransactionStatus.Active;
                recurringTransaction.OccurrenceCount = 0;
                recurringTransaction.CreatedDate = DateTime.UtcNow;
                recurringTransaction.LastModifiedDate = DateTime.UtcNow;

                _context.RecurringTransactions.Add(recurringTransaction);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully created recurring transaction {Id} for user {UserId}", 
                    recurringTransaction.Id, userId);

                return _mapper.Map<RecurringTransactionModel>(recurringTransaction);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create recurring transaction for user: {UserId}", userId);
                throw new Exception("An error occurred while creating the recurring transaction.", ex);
            }
        }

        public async Task<RecurringTransactionModel> UpdateRecurringTransaction(Guid id, UpdateRecurringTransactionModel updateModel, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingRecurringTransaction = await _context.RecurringTransactions
                    .FirstOrDefaultAsync(rt => rt.Id == id && rt.UserId == userId);

                if (existingRecurringTransaction == null)
                {
                    throw new KeyNotFoundException("Recurring transaction not found");
                }

                // Validate the model
                ValidateUpdateRecurringTransactionModel(updateModel);

                // Update the recurring transaction
                _mapper.Map(updateModel, existingRecurringTransaction);
                existingRecurringTransaction.Category = updateModel.Category ?? existingRecurringTransaction.Category;
                existingRecurringTransaction.LastModifiedDate = DateTime.UtcNow;

                // Recalculate next occurrence date if frequency or start date changed
                if (existingRecurringTransaction.Frequency != updateModel.Frequency || 
                    existingRecurringTransaction.StartDate != updateModel.StartDate)
                {
                    existingRecurringTransaction.NextOccurrenceDate = await CalculateNextOccurrenceDate(
                        updateModel.StartDate, updateModel.Frequency.ToString());
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully updated recurring transaction {Id} for user {UserId}", 
                    id, userId);

                return _mapper.Map<RecurringTransactionModel>(existingRecurringTransaction);
            }
            catch (KeyNotFoundException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Recurring transaction not found with id: {Id} for user: {UserId}", id, userId);
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update recurring transaction {Id} for user: {UserId}", id, userId);
                throw new Exception("An error occurred while updating the recurring transaction.", ex);
            }
        }

        public async Task<RecurringTransactionModel> UpdateRecurringTransactionStatus(Guid id, UpdateRecurringTransactionStatusModel statusModel, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingRecurringTransaction = await _context.RecurringTransactions
                    .FirstOrDefaultAsync(rt => rt.Id == id && rt.UserId == userId);

                if (existingRecurringTransaction == null)
                {
                    throw new KeyNotFoundException("Recurring transaction not found");
                }

                existingRecurringTransaction.Status = statusModel.Status;
                existingRecurringTransaction.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully updated status of recurring transaction {Id} to {Status} for user {UserId}", 
                    id, statusModel.Status, userId);

                return _mapper.Map<RecurringTransactionModel>(existingRecurringTransaction);
            }
            catch (KeyNotFoundException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogWarning(ex, "Recurring transaction not found with id: {Id} for user: {UserId}", id, userId);
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to update status of recurring transaction {Id} for user: {UserId}", id, userId);
                throw new Exception("An error occurred while updating the recurring transaction status.", ex);
            }
        }

        public async Task<bool> DeleteRecurringTransaction(Guid id, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingRecurringTransaction = await _context.RecurringTransactions
                    .FirstOrDefaultAsync(rt => rt.Id == id && rt.UserId == userId);

                if (existingRecurringTransaction == null)
                {
                    return false;
                }

                _context.RecurringTransactions.Remove(existingRecurringTransaction);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully deleted recurring transaction {Id} for user {UserId}", 
                    id, userId);

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to delete recurring transaction {Id} for user: {UserId}", id, userId);
                throw new Exception("An error occurred while deleting the recurring transaction.", ex);
            }
        }

        public async Task<RecurringTransactionSummaryModel> GetRecurringTransactionSummary(string userId)
        {
            try
            {
                var recurringTransactions = await _context.RecurringTransactions
                    .Where(rt => rt.UserId == userId)
                    .ToListAsync();

                var summary = new RecurringTransactionSummaryModel
                {
                    TotalRecurringTransactions = recurringTransactions.Count,
                    ActiveRecurringTransactions = recurringTransactions.Count(rt => rt.Status == RecurringTransactionStatus.Active),
                    PausedRecurringTransactions = recurringTransactions.Count(rt => rt.Status == RecurringTransactionStatus.Paused),
                    CancelledRecurringTransactions = recurringTransactions.Count(rt => rt.Status == RecurringTransactionStatus.Cancelled),
                    TotalMonthlyAmount = recurringTransactions
                        .Where(rt => rt.Status == RecurringTransactionStatus.Active && rt.Frequency == RenewalFrequency.Monthly)
                        .Sum(rt => rt.Amount),
                    TotalWeeklyAmount = recurringTransactions
                        .Where(rt => rt.Status == RecurringTransactionStatus.Active && rt.Frequency == RenewalFrequency.Weekly)
                        .Sum(rt => rt.Amount),
                    TotalYearlyAmount = recurringTransactions
                        .Where(rt => rt.Status == RecurringTransactionStatus.Active && rt.Frequency == RenewalFrequency.Yearly)
                        .Sum(rt => rt.Amount),
                    RecentRecurringTransactions = _mapper.Map<List<RecurringTransactionModel>>(
                        recurringTransactions.OrderByDescending(rt => rt.CreatedDate).Take(5))
                };

                // Calculate category and type breakdowns
                summary.CategoryBreakdown = recurringTransactions
                    .Where(rt => rt.Status == RecurringTransactionStatus.Active)
                    .GroupBy(rt => rt.Category)
                    .ToDictionary(g => g.Key, g => g.Sum(rt => rt.Amount));

                summary.TypeBreakdown = recurringTransactions
                    .Where(rt => rt.Status == RecurringTransactionStatus.Active)
                    .GroupBy(rt => rt.Type)
                    .ToDictionary(g => g.Key, g => g.Count());

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving recurring transaction summary for user: {UserId}", userId);
                throw new Exception("An error occurred while retrieving the recurring transaction summary.", ex);
            }
        }

        public async Task<IEnumerable<RecurringTransactionModel>> GetRecurringTransactionsByStatus(string userId, string status)
        {
            try
            {
                if (!Enum.TryParse<RecurringTransactionStatus>(status, true, out var statusEnum))
                {
                    throw new ArgumentException("Invalid status value");
                }

                var recurringTransactions = await _context.RecurringTransactions
                    .Where(rt => rt.UserId == userId && rt.Status == statusEnum)
                    .OrderByDescending(rt => rt.CreatedDate)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<RecurringTransactionModel>>(recurringTransactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving recurring transactions by status for user: {UserId}", userId);
                throw new Exception("An error occurred while retrieving recurring transactions by status.", ex);
            }
        }

        public async Task<IEnumerable<RecurringTransactionModel>> GetRecurringTransactionsByFrequency(string userId, string frequency)
        {
            try
            {
                if (!Enum.TryParse<RenewalFrequency>(frequency, true, out var frequencyEnum))
                {
                    throw new ArgumentException("Invalid frequency value");
                }

                var recurringTransactions = await _context.RecurringTransactions
                    .Where(rt => rt.UserId == userId && rt.Frequency == frequencyEnum)
                    .OrderByDescending(rt => rt.CreatedDate)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<RecurringTransactionModel>>(recurringTransactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving recurring transactions by frequency for user: {UserId}", userId);
                throw new Exception("An error occurred while retrieving recurring transactions by frequency.", ex);
            }
        }

        public async Task<bool> PauseRecurringTransaction(Guid id, string userId)
        {
            return await UpdateRecurringTransactionStatus(id, new UpdateRecurringTransactionStatusModel 
            { 
                Status = RecurringTransactionStatus.Paused 
            }, userId) != null;
        }

        public async Task<bool> ResumeRecurringTransaction(Guid id, string userId)
        {
            return await UpdateRecurringTransactionStatus(id, new UpdateRecurringTransactionStatusModel 
            { 
                Status = RecurringTransactionStatus.Active 
            }, userId) != null;
        }

        public async Task<bool> CancelRecurringTransaction(Guid id, string userId)
        {
            return await UpdateRecurringTransactionStatus(id, new UpdateRecurringTransactionStatusModel 
            { 
                Status = RecurringTransactionStatus.Cancelled 
            }, userId) != null;
        }

        public async Task<DateTime> CalculateNextOccurrenceDate(DateTime currentDate, string frequency)
        {
            if (!Enum.TryParse<RenewalFrequency>(frequency, true, out var frequencyEnum))
            {
                throw new ArgumentException("Invalid frequency value");
            }

            return frequencyEnum switch
            {
                RenewalFrequency.Weekly => currentDate.AddDays(7),
                RenewalFrequency.Monthly => currentDate.AddMonths(1),
                RenewalFrequency.Yearly => currentDate.AddYears(1),
                _ => currentDate.AddMonths(1) // Default to monthly
            };
        }

        public async Task<IEnumerable<RecurringTransactionModel>> GetRecurringTransactionsReadyForProcessing()
        {
            try
            {
                var now = DateTime.UtcNow;
                var readyTransactions = await _context.RecurringTransactions
                    .Where(rt => rt.Status == RecurringTransactionStatus.Active &&
                                rt.NextOccurrenceDate <= now &&
                                (rt.EndDate == null || rt.EndDate > now))
                    .ToListAsync();

                return _mapper.Map<IEnumerable<RecurringTransactionModel>>(readyTransactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving recurring transactions ready for processing");
                throw new Exception("An error occurred while retrieving recurring transactions ready for processing.", ex);
            }
        }

        private void ValidateCreateRecurringTransactionModel(CreateRecurringTransactionModel model)
        {
            if (model.StartDate < DateTime.UtcNow.Date)
            {
                throw new ArgumentException("Start date cannot be in the past");
            }

            if (model.EndDate.HasValue && model.EndDate.Value <= model.StartDate)
            {
                throw new ArgumentException("End date must be after start date");
            }

            if (model.Date < DateTime.UtcNow.Date)
            {
                throw new ArgumentException("Transaction date cannot be in the past");
            }
        }

        private void ValidateUpdateRecurringTransactionModel(UpdateRecurringTransactionModel model)
        {
            if (model.StartDate < DateTime.UtcNow.Date)
            {
                throw new ArgumentException("Start date cannot be in the past");
            }

            if (model.EndDate.HasValue && model.EndDate.Value <= model.StartDate)
            {
                throw new ArgumentException("End date must be after start date");
            }

            if (model.Date < DateTime.UtcNow.Date)
            {
                throw new ArgumentException("Transaction date cannot be in the past");
            }
        }

        private DateTime CalculateNextOccurrenceDate(DateTime startDate, RenewalFrequency frequency)
        {
            return frequency switch
            {
                RenewalFrequency.Weekly => startDate.AddDays(7),
                RenewalFrequency.Monthly => startDate.AddMonths(1),
                RenewalFrequency.Yearly => startDate.AddYears(1),
                _ => startDate.AddMonths(1) // Default to monthly
            };
        }
    }
}
