using AutoMapper;
using FinDepen_Backend.Constants;
using FinDepen_Backend.Data;
using FinDepen_Backend.Entities;
using FinDepen_Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinDepen_Backend.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<BudgetService> _logger;
        
        public BudgetService(ApplicationDbContext context, IMapper mapper, ILogger<BudgetService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<Budget>> GetBudgets(string userId)
        {
            try
            {
                _logger.LogInformation("Retrieving budgets for user: {UserId}", userId);
                
                var budgets = await _context.Budgets
                .Where(b => b.UserId == userId)
                    .OrderBy(b => b.Category)
                .ToListAsync();
                
                _logger.LogInformation("Successfully retrieved {Count} budgets for user: {UserId}", budgets.Count(), userId);
                return budgets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving budgets for user: {UserId}", userId);
                throw new Exception("An error occurred while retrieving budgets.", ex);
            }
        }

        public async Task<Budget> GetBudgetById(Guid id)
        {
            try
            {
                _logger.LogInformation("Retrieving budget with ID: {BudgetId}", id);
                
                var budget = await _context.Budgets
                    .FirstOrDefaultAsync(b => b.Id == id);
                
                if (budget == null)
                {
                    _logger.LogWarning("Budget with ID {BudgetId} not found", id);
                    throw new KeyNotFoundException($"Budget with ID {id} not found.");
                }
                
                _logger.LogInformation("Successfully retrieved budget with ID: {BudgetId}", id);
                return budget;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving budget with ID: {BudgetId}", id);
                throw new Exception("An error occurred while retrieving the budget.", ex);
            }
        }

        public async Task<Budget> CreateBudget(Budget budget)
        {
            try
            {
                _logger.LogInformation("Creating new budget for user: {UserId}, Category: {Category}", budget.UserId, budget.Category);
                
                ValidateBudgetData(budget);
                
                budget.Id = Guid.NewGuid();
                budget.SpentAmount = 0; // Initialize spent amount to 0 for new budgets
                
                _context.Budgets.Add(budget);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully created budget with ID: {BudgetId} for user: {UserId}", budget.Id, budget.UserId);
                return budget;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating budget for user: {UserId}", budget.UserId);
                throw new Exception("An error occurred while creating the budget.", ex);
            }
        }

        public async Task<Budget> UpdateBudget(Guid id, Budget updatedBudget)
        {
            try
            {
                _logger.LogInformation("Updating budget with ID: {BudgetId}", id);
                
                var existingBudget = await GetBudgetById(id);
                ValidateBudgetData(updatedBudget);
                
                UpdateBudgetProperties(existingBudget, updatedBudget);
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully updated budget with ID: {BudgetId}", id);
                return existingBudget;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating budget with ID: {BudgetId}", id);
                throw new Exception("An error occurred while updating the budget.", ex);
        }
        }

        public async Task<Budget> DeleteBudget(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting budget with ID: {BudgetId}", id);
                
                var budget = await GetBudgetById(id);
                
                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully deleted budget with ID: {BudgetId}", id);
                return budget;
            }
            catch (KeyNotFoundException)
            {
                throw;
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting budget with ID: {BudgetId}", id);
                throw new Exception("An error occurred while deleting the budget.", ex);
            }
        }

        private void ValidateBudgetData(Budget budget)
        {
            if (string.IsNullOrWhiteSpace(budget.Category))
            {
                throw new ArgumentException("Category is required.");
            }
            
            if (!Categories.IsValidCategory(budget.Category))
            {
                throw new ArgumentException($"Category must be one of: {Categories.GetValidCategoriesString()}");
            }
            
            if (budget.PlannedAmount <= 0)
            {
                throw new ArgumentException("Planned amount must be greater than 0.");
        }
            
            if (budget.SpentAmount < 0)
            {
                throw new ArgumentException("Spent amount cannot be negative.");
            }
        }

        private void UpdateBudgetProperties(Budget existingBudget, Budget updatedBudget)
        {
            existingBudget.Category = updatedBudget.Category;
            existingBudget.PlannedAmount = updatedBudget.PlannedAmount;
            existingBudget.SpentAmount = updatedBudget.SpentAmount;
            existingBudget.Reminder = updatedBudget.Reminder;
            existingBudget.StartDate = updatedBudget.StartDate;
            existingBudget.RenewalFrequency = updatedBudget.RenewalFrequency;
        }
    }
}
