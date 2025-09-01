using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FinDepen_Backend.Data;
using FinDepen_Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinDepen_Backend.Services
{
    public class BudgetAutoRenewalService : BackgroundService, IBudgetAutoRenewalService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BudgetAutoRenewalService> _logger;
        private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24); // Run daily

        public BudgetAutoRenewalService(IServiceProvider serviceProvider, ILogger<BudgetAutoRenewalService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Budget auto-renewal service started");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessBudgetRenewals(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during budget auto-renewal process.");
                }

                await Task.Delay(RunInterval, stoppingToken);
            }
        }

        private async Task ProcessBudgetRenewals(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var now = DateTime.UtcNow;
            var budgetsToRenew = await GetEligibleBudgetsForRenewal(dbContext, now, stoppingToken);

            if (!budgetsToRenew.Any())
            {
                _logger.LogDebug("No budgets eligible for renewal at {CurrentTime}", now);
                return;
            }

            _logger.LogInformation("Found {Count} budgets eligible for renewal", budgetsToRenew.Count);

            foreach (var oldBudget in budgetsToRenew)
            {
                try
                {
                    await RenewBudget(dbContext, oldBudget, now, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to renew budget {BudgetId} for user {UserId}", 
                        oldBudget.Id, oldBudget.UserId);
                }
            }

            await dbContext.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Completed processing {Count} budget renewals", budgetsToRenew.Count);
        }

        private async Task<List<Budget>> GetEligibleBudgetsForRenewal(ApplicationDbContext dbContext, DateTime now, CancellationToken stoppingToken)
        {
            return await dbContext.Budgets
                .Where(b => b.AutoRenewalEnabled && 
                           b.EndDate != null && 
                           b.EndDate <= now &&
                           b.UserId != null) // Ensure user exists
                .ToListAsync(stoppingToken);
        }

        private async Task RenewBudget(ApplicationDbContext dbContext, Budget oldBudget, DateTime now, CancellationToken stoppingToken)
        {
            // Validate budget data before renewal
            if (!IsValidBudgetForRenewal(oldBudget))
            {
                _logger.LogWarning("Budget {BudgetId} is not valid for renewal", oldBudget.Id);
                return;
            }

            var newStartDate = oldBudget.EndDate!.Value; // Use null-forgiving operator since we validated EndDate is not null
            var newEndDate = GetNextEndDate(newStartDate, oldBudget.RenewalFrequency);

            var newBudget = CreateNewBudgetFromOld(oldBudget, newStartDate, newEndDate, now);

            // Use transaction to ensure data consistency
            using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);
            try
            {
                dbContext.Budgets.Add(newBudget);
                
                // Disable auto-renewal on old budget to prevent duplicate renewals
                oldBudget.AutoRenewalEnabled = false;
                oldBudget.LastRenewalDate = now;

                await dbContext.SaveChangesAsync(stoppingToken);
                await transaction.CommitAsync(stoppingToken);

                _logger.LogInformation("Successfully renewed budget {OldBudgetId} -> {NewBudgetId} for user {UserId}", 
                    oldBudget.Id, newBudget.Id, oldBudget.UserId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(stoppingToken);
                _logger.LogError(ex, "Failed to renew budget {BudgetId}, transaction rolled back", oldBudget.Id);
                throw;
            }
        }

        private bool IsValidBudgetForRenewal(Budget budget)
        {
            if (budget == null) return false;
            if (string.IsNullOrWhiteSpace(budget.Category)) return false;
            if (budget.PlannedAmount <= 0) return false;
            if (budget.EndDate == null) return false;
            if (string.IsNullOrWhiteSpace(budget.UserId)) return false;
            if (!budget.AutoRenewalEnabled) return false;
            
            return true;
        }

        private Budget CreateNewBudgetFromOld(Budget oldBudget, DateTime newStartDate, DateTime newEndDate, DateTime renewalTime)
        {
            return new Budget
            {
                Id = Guid.NewGuid(),
                Category = oldBudget.Category,
                PlannedAmount = oldBudget.PlannedAmount,
                SpentAmount = 0, // Reset spent amount for new period
                Reminder = oldBudget.Reminder,
                StartDate = newStartDate,
                EndDate = newEndDate,
                RenewalFrequency = oldBudget.RenewalFrequency,
                AutoRenewalEnabled = true, // Keep auto-renewal enabled for next period
                LastRenewalDate = renewalTime,
                RenewalCount = oldBudget.RenewalCount + 1,
                UserId = oldBudget.UserId
            };
        }

        private DateTime GetNextEndDate(DateTime startDate, Constants.RenewalFrequency frequency)
        {
            return frequency switch
            {
                Constants.RenewalFrequency.Weekly => startDate.AddDays(7),
                Constants.RenewalFrequency.Monthly => startDate.AddMonths(1),
                Constants.RenewalFrequency.Yearly => startDate.AddYears(1),
                _ => startDate.AddMonths(1) // Default to monthly
            };
        }

        // Method for testing purposes - manually trigger renewal
        public async Task<bool> TestRenewalForBudget(Guid budgetId, CancellationToken stoppingToken = default)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                var budget = await dbContext.Budgets.FindAsync(budgetId);
                if (budget == null)
                {
                    _logger.LogWarning("Budget {BudgetId} not found for test renewal", budgetId);
                    return false;
                }

                await RenewBudget(dbContext, budget, DateTime.UtcNow, stoppingToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test renewal failed for budget {BudgetId}", budgetId);
                return false;
            }
        }
    }
} 