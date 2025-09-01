using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FinDepen_Backend.Data;
using FinDepen_Backend.Entities;
using FinDepen_Backend.DTOs;
using FinDepen_Backend.Constants;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FinDepen_Backend.Services
{
    public class RecurringTransactionProcessingService : BackgroundService, IRecurringTransactionProcessingService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RecurringTransactionProcessingService> _logger;
        private static readonly TimeSpan RunInterval = TimeSpan.FromHours(1); // Run every hour

        public RecurringTransactionProcessingService(IServiceProvider serviceProvider, ILogger<RecurringTransactionProcessingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Recurring transaction processing service started");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRecurringTransactions();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during recurring transaction processing.");
                }

                await Task.Delay(RunInterval, stoppingToken);
            }
        }

        public async Task ProcessRecurringTransactions()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var recurringTransactionService = scope.ServiceProvider.GetRequiredService<IRecurringTransactionService>();
            
            var now = DateTime.UtcNow;
            var readyTransactions = await GetRecurringTransactionsReadyForProcessing(dbContext, now);

            if (!readyTransactions.Any())
            {
                _logger.LogDebug("No recurring transactions ready for processing at {CurrentTime}", now);
                return;
            }

            _logger.LogInformation("Found {Count} recurring transactions ready for processing", readyTransactions.Count);

            foreach (var recurringTransaction in readyTransactions)
            {
                try
                {
                    await ProcessRecurringTransaction(recurringTransaction, dbContext, now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process recurring transaction {RecurringTransactionId} for user {UserId}", 
                        recurringTransaction.Id, recurringTransaction.UserId);
                }
            }

            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Completed processing {Count} recurring transactions", readyTransactions.Count);
        }

        public async Task ProcessRecurringTransaction(Guid recurringTransactionId)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var recurringTransaction = await dbContext.RecurringTransactions
                .FirstOrDefaultAsync(rt => rt.Id == recurringTransactionId);

            if (recurringTransaction == null)
            {
                _logger.LogWarning("Recurring transaction {RecurringTransactionId} not found", recurringTransactionId);
                return;
            }

            if (!recurringTransaction.CanBeProcessed)
            {
                _logger.LogWarning("Recurring transaction {RecurringTransactionId} is not ready for processing", recurringTransactionId);
                return;
            }

            await ProcessRecurringTransaction(recurringTransaction, dbContext, DateTime.UtcNow);
            await dbContext.SaveChangesAsync();
        }

        private async Task<List<RecurringTransaction>> GetRecurringTransactionsReadyForProcessing(ApplicationDbContext dbContext, DateTime now)
        {
            return await dbContext.RecurringTransactions
                .Where(rt => rt.Status == RecurringTransactionStatus.Active &&
                           rt.NextOccurrenceDate <= now &&
                           (rt.EndDate == null || rt.EndDate > now))
                .ToListAsync();
        }

        private async Task ProcessRecurringTransaction(RecurringTransaction recurringTransaction, ApplicationDbContext dbContext, DateTime processingTime)
        {
            // Validate recurring transaction before processing
            if (!IsValidRecurringTransactionForProcessing(recurringTransaction))
            {
                _logger.LogWarning("Recurring transaction {RecurringTransactionId} is not valid for processing", recurringTransaction.Id);
                return;
            }

            // Create the new transaction
            var newTransaction = CreateTransactionFromRecurring(recurringTransaction, processingTime);

            // Use transaction to ensure data consistency
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                // Add the new transaction
                dbContext.Transactions.Add(newTransaction);

                // Update user balance
                var user = await dbContext.Users.FindAsync(recurringTransaction.UserId);
                if (user != null)
                {
                    if (recurringTransaction.Type == "Income")
                    {
                        user.BalanceAmount += recurringTransaction.Amount;
                    }
                    else
                    {
                        // Validate balance for expense transactions
                        if (user.BalanceAmount < recurringTransaction.Amount)
                        {
                            throw new InvalidOperationException($"Insufficient balance for recurring transaction. Current balance: {user.BalanceAmount:C}, Transaction amount: {recurringTransaction.Amount:C}");
                        }
                        user.BalanceAmount -= recurringTransaction.Amount;
                    }
                }

                // Update recurring transaction
                UpdateRecurringTransactionAfterProcessing(recurringTransaction, processingTime);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully processed recurring transaction {RecurringTransactionId} -> {NewTransactionId} for user {UserId}", 
                    recurringTransaction.Id, newTransaction.Id, recurringTransaction.UserId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to process recurring transaction {RecurringTransactionId}, transaction rolled back", recurringTransaction.Id);
                throw;
            }
        }

        private bool IsValidRecurringTransactionForProcessing(RecurringTransaction recurringTransaction)
        {
            if (recurringTransaction == null) return false;
            if (string.IsNullOrWhiteSpace(recurringTransaction.Title)) return false;
            if (recurringTransaction.Amount <= 0) return false;
            if (string.IsNullOrWhiteSpace(recurringTransaction.Category)) return false;
            if (string.IsNullOrWhiteSpace(recurringTransaction.Type)) return false;
            if (string.IsNullOrWhiteSpace(recurringTransaction.UserId)) return false;
            if (recurringTransaction.Status != RecurringTransactionStatus.Active) return false;
            if (recurringTransaction.NextOccurrenceDate > DateTime.UtcNow) return false;
            if (recurringTransaction.EndDate.HasValue && recurringTransaction.EndDate.Value <= DateTime.UtcNow) return false;
            
            return true;
        }

        private Transaction CreateTransactionFromRecurring(RecurringTransaction recurringTransaction, DateTime processingTime)
        {
            return new Transaction
            {
                Id = Guid.NewGuid(),
                Title = recurringTransaction.Title,
                Description = recurringTransaction.Description,
                Amount = recurringTransaction.Amount,
                Category = recurringTransaction.Category,
                Type = recurringTransaction.Type,
                Date = processingTime.Date, // Use the processing date
                UserId = recurringTransaction.UserId,
                BudgetId = recurringTransaction.BudgetId,
                IsRecurringGenerated = true,
                RecurringTransactionId = recurringTransaction.Id
            };
        }

        private void UpdateRecurringTransactionAfterProcessing(RecurringTransaction recurringTransaction, DateTime processingTime)
        {
            // Increment occurrence count
            recurringTransaction.OccurrenceCount++;

            // Update last created date
            recurringTransaction.LastCreatedDate = processingTime;

            // Calculate next occurrence date based on the start date and occurrence count
            // This ensures we always calculate from the original start date
            var nextOccurrence = recurringTransaction.StartDate;
            
            // Add the frequency multiple times based on occurrence count (including this one)
            for (int i = 0; i <= recurringTransaction.OccurrenceCount; i++)
            {
                nextOccurrence = CalculateNextOccurrenceDate(nextOccurrence, recurringTransaction.Frequency);
            }
            
            recurringTransaction.NextOccurrenceDate = nextOccurrence;

            // Update last modified date
            recurringTransaction.LastModifiedDate = processingTime;

            // Check if we've reached the end date
            if (recurringTransaction.EndDate.HasValue && 
                recurringTransaction.NextOccurrenceDate > recurringTransaction.EndDate.Value)
            {
                recurringTransaction.Status = RecurringTransactionStatus.Cancelled;
                _logger.LogInformation("Recurring transaction {RecurringTransactionId} has reached its end date and has been cancelled", 
                    recurringTransaction.Id);
            }
        }

        private DateTime CalculateNextOccurrenceDate(DateTime currentDate, RenewalFrequency frequency)
        {
            return frequency switch
            {
                RenewalFrequency.Weekly => currentDate.AddDays(7),
                RenewalFrequency.Monthly => currentDate.AddMonths(1),
                RenewalFrequency.Yearly => currentDate.AddYears(1),
                _ => currentDate.AddMonths(1) // Default to monthly
            };
        }
    }
}
