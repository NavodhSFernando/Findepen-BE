using AutoMapper;
using FinDepen_Backend.Constants;
using FinDepen_Backend.Data;
using FinDepen_Backend.DTOs;
using FinDepen_Backend.Entities;
using FinDepen_Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinDepen_Backend.Services
{
    public class GoalService : IGoalService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<GoalService> _logger;
        
        public GoalService(ApplicationDbContext context, IMapper mapper, ILogger<GoalService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<Goal>> GetGoals(string userId)
        {
            try
            {
                _logger.LogInformation("Retrieving goals for user: {UserId}", userId);
                
                var goals = await _context.Goals
                    .Where(g => g.UserId == userId)
                    .OrderBy(g => g.Priority)
                    .ThenBy(g => g.TargetDate)
                    .ToListAsync();
                
                _logger.LogInformation("Successfully retrieved {Count} goals for user: {UserId}", goals.Count(), userId);
                return goals;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving goals for user: {UserId}", userId);
                throw new Exception("An error occurred while retrieving goals.", ex);
            }
        }

        public async Task<Goal> GetGoalById(Guid id)
        {
            try
            {
                _logger.LogInformation("Retrieving goal with ID: {GoalId}", id);
                
                var goal = await _context.Goals
                    .FirstOrDefaultAsync(g => g.Id == id);
                
                if (goal == null)
                {
                    _logger.LogWarning("Goal with ID {GoalId} not found", id);
                    throw new KeyNotFoundException($"Goal with ID {id} not found.");
                }
                
                _logger.LogInformation("Successfully retrieved goal with ID: {GoalId}", id);
                return goal;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving goal with ID: {GoalId}", id);
                throw new Exception("An error occurred while retrieving the goal.", ex);
            }
        }

        public async Task<Goal> CreateGoal(Goal goal)
        {
            try
            {
                _logger.LogInformation("Creating new goal for user: {UserId}, Title: {Title}", goal.UserId, goal.Title);
                
                ValidateGoalData(goal);
                
                goal.Id = Guid.NewGuid();
                goal.CurrentAmount = 0; // Initialize current amount to 0 for new goals
                goal.CreatedDate = DateTime.UtcNow;
                goal.LastUpdatedDate = DateTime.UtcNow;
                goal.Status = GoalStatus.Active;
                goal.IsActive = true;
                
                _logger.LogInformation("Creating goal with Status: {Status}, IsActive: {IsActive}", goal.Status, goal.IsActive);
                
                _context.Goals.Add(goal);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully created goal with ID: {GoalId} for user: {UserId}", goal.Id, goal.UserId);
                return goal;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating goal for user: {UserId}", goal.UserId);
                throw new Exception("An error occurred while creating the goal.", ex);
            }
        }

        public async Task<Goal> UpdateGoal(Guid id, Goal updatedGoal)
        {
            try
            {
                _logger.LogInformation("Updating goal with ID: {GoalId}", id);
                
                var existingGoal = await GetGoalById(id);
                ValidateGoalData(updatedGoal);
                
                UpdateGoalProperties(existingGoal, updatedGoal);
                existingGoal.LastUpdatedDate = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully updated goal with ID: {GoalId}", id);
                return existingGoal;
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
                _logger.LogError(ex, "An error occurred while updating goal with ID: {GoalId}", id);
                throw new Exception("An error occurred while updating the goal.", ex);
            }
        }

        public async Task<Goal> DeleteGoal(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting goal with ID: {GoalId}", id);
                
                var goal = await GetGoalById(id);
                
                _context.Goals.Remove(goal);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully deleted goal with ID: {GoalId}", id);
                return goal;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting goal with ID: {GoalId}", id);
                throw new Exception("An error occurred while deleting the goal.", ex);
            }
        }

        public async Task<Goal> AddFundsToGoal(Guid goalId, double amount, string userId)
        {
            try
            {
                _logger.LogInformation("Adding {Amount} funds to goal {GoalId} for user {UserId}", amount, goalId, userId);
                
                var goal = await GetGoalById(goalId);
                
                // Verify the goal belongs to the user
                if (goal.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to access goal {GoalId} belonging to user {GoalUserId}", userId, goalId, goal.UserId);
                    throw new UnauthorizedAccessException("You can only modify your own goals.");
                }
                
                // Check if user has sufficient balance
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found.");
                }
                
                if (user.BalanceAmount < amount)
                {
                    throw new InvalidOperationException("Insufficient balance to add funds to goal.");
                }
                
                // Update goal and user balance
                goal.CurrentAmount += amount;
                goal.LastUpdatedDate = DateTime.UtcNow;
                user.BalanceAmount -= amount;
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully added {Amount} funds to goal {GoalId}", amount, goalId);
                return goal;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding funds to goal {GoalId}", goalId);
                throw new Exception("An error occurred while adding funds to the goal.", ex);
            }
        }

        public async Task<Goal> WithdrawFundsFromGoal(Guid goalId, double amount, string userId)
        {
            try
            {
                _logger.LogInformation("Withdrawing {Amount} funds from goal {GoalId} for user {UserId}", amount, goalId, userId);
                
                var goal = await GetGoalById(goalId);
                
                // Verify the goal belongs to the user
                if (goal.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to access goal {GoalId} belonging to user {GoalUserId}", userId, goalId, goal.UserId);
                    throw new UnauthorizedAccessException("You can only modify your own goals.");
                }
                
                // Check if goal has sufficient funds
                if (goal.CurrentAmount < amount)
                {
                    throw new InvalidOperationException("Insufficient funds in goal to withdraw.");
                }
                
                // Update goal and user balance
                goal.CurrentAmount -= amount;
                goal.LastUpdatedDate = DateTime.UtcNow;
                
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    user.BalanceAmount += amount;
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully withdrew {Amount} funds from goal {GoalId}", amount, goalId);
                return goal;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while withdrawing funds from goal {GoalId}", goalId);
                throw new Exception("An error occurred while withdrawing funds from the goal.", ex);
            }
        }

        public async Task<Goal> ConvertGoalToExpense(Guid goalId, double amount, string transactionTitle, string? transactionDescription, string category, string userId)
        {
            try
            {
                _logger.LogInformation("Converting {Amount} funds from goal {GoalId} to expense for user {UserId}", amount, goalId, userId);
                
                var goal = await GetGoalById(goalId);
                
                // Verify the goal belongs to the user
                if (goal.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to access goal {GoalId} belonging to user {GoalUserId}", userId, goalId, goal.UserId);
                    throw new UnauthorizedAccessException("You can only modify your own goals.");
                }
                
                // Check if goal has sufficient funds
                if (goal.CurrentAmount < amount)
                {
                    throw new InvalidOperationException("Insufficient funds in goal to convert to expense.");
                }
                
                // Validate category
                if (!Categories.IsValidCategory(category))
                {
                    throw new ArgumentException($"Category must be one of: {Categories.GetValidCategoriesString()}");
                }
                
                // Create transaction
                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    Title = transactionTitle,
                    Description = transactionDescription,
                    Amount = amount,
                    Category = category,
                    Type = "Expense",
                    Date = DateTime.UtcNow,
                    UserId = userId
                };
                
                // Update goal
                goal.CurrentAmount -= amount;
                goal.LastUpdatedDate = DateTime.UtcNow;
                goal.IsActive = false;
                goal.Status = GoalStatus.Completed;
                
                // Add transaction to context
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully converted {Amount} funds from goal {GoalId} to expense", amount, goalId);
                return goal;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while converting goal {GoalId} to expense", goalId);
                throw new Exception("An error occurred while converting the goal to expense.", ex);
            }
        }

        public async Task<GoalSummaryModel> GetGoalSummary(string userId)
        {
            try
            {
                _logger.LogInformation("Retrieving goal summary for user: {UserId}", userId);
                
                var goals = await GetGoals(userId);
                var goalList = goals.ToList();
                
                var activeGoalsCount = goalList.Count(g => g.IsActive && g.Status == GoalStatus.Active);
                _logger.LogInformation("Goal summary calculation - Total: {Total}, Active: {Active}, Goals with IsActive=true: {IsActiveCount}, Goals with Status=Active: {StatusActiveCount}", 
                    goalList.Count, activeGoalsCount, 
                    goalList.Count(g => g.IsActive), 
                    goalList.Count(g => g.Status == GoalStatus.Active));
                
                // Log each goal's status for debugging
                foreach (var goal in goalList)
                {
                    _logger.LogInformation("Goal {Id}: Status={Status}, IsActive={IsActive}", goal.Id, goal.Status, goal.IsActive);
                }
                
                var summary = new GoalSummaryModel
                {
                    TotalGoals = goalList.Count,
                    ActiveGoals = activeGoalsCount,
                    CompletedGoals = goalList.Count(g => g.Status == GoalStatus.Completed || g.CurrentAmount >= g.TargetAmount),
                    OverdueGoals = goalList.Count(g => DateTime.UtcNow > g.TargetDate && g.CurrentAmount < g.TargetAmount),
                    TotalTargetAmount = goalList.Sum(g => g.TargetAmount),
                    TotalCurrentAmount = goalList.Sum(g => g.CurrentAmount),
                    TotalRemainingAmount = goalList.Sum(g => Math.Max(g.TargetAmount - g.CurrentAmount, 0)),
                    OverallProgressPercentage = goalList.Any() ? 
                        (goalList.Sum(g => g.CurrentAmount) / goalList.Sum(g => g.TargetAmount)) * 100 : 0,
                    TotalMonthlyRequired = goalList.Sum(g => 
                    {
                        var monthsRemaining = Math.Max((g.TargetDate - DateTime.UtcNow).Days / 30.0, 1);
                        return Math.Max((g.TargetAmount - g.CurrentAmount) / monthsRemaining, 0);
                    }),
                    TotalWeeklyRequired = goalList.Sum(g => 
                    {
                        var weeksRemaining = Math.Max((g.TargetDate - DateTime.UtcNow).Days / 7.0, 1);
                        return Math.Max((g.TargetAmount - g.CurrentAmount) / weeksRemaining, 0);
                    }),
                    PriorityBreakdown = goalList.GroupBy(g => g.Priority)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    StatusBreakdown = goalList.GroupBy(g => g.Status)
                        .ToDictionary(g => g.Key, g => g.Count())
                };
                
                _logger.LogInformation("Successfully retrieved goal summary for user: {UserId}", userId);
                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving goal summary for user: {UserId}", userId);
                throw new Exception("An error occurred while retrieving the goal summary.", ex);
            }
        }

        private void ValidateGoalData(Goal goal)
        {
            if (string.IsNullOrWhiteSpace(goal.Title))
            {
                throw new ArgumentException("Title is required.");
            }
            
            if (goal.Title.Length < 2 || goal.Title.Length > 100)
            {
                throw new ArgumentException("Title must be between 2 and 100 characters.");
            }
            
            if (goal.TargetAmount <= 0)
            {
                throw new ArgumentException("Target amount must be greater than 0.");
            }
            
            if (goal.CurrentAmount < 0)
            {
                throw new ArgumentException("Current amount cannot be negative.");
            }
            
            // Priority and Status are enums, so no validation needed for their values
        }

        private void UpdateGoalProperties(Goal existingGoal, Goal updatedGoal)
        {
            existingGoal.Title = updatedGoal.Title;
            existingGoal.Description = updatedGoal.Description;
            existingGoal.TargetAmount = updatedGoal.TargetAmount;
            // Only update CurrentAmount if it's explicitly provided and greater than 0
            if (updatedGoal.CurrentAmount > 0)
            {
                existingGoal.CurrentAmount = updatedGoal.CurrentAmount;
            }
            existingGoal.TargetDate = updatedGoal.TargetDate;
            existingGoal.Priority = updatedGoal.Priority;
            // GoalType removed, no longer needed
            existingGoal.IsActive = updatedGoal.IsActive;
            existingGoal.Reminder = updatedGoal.Reminder;
            existingGoal.Status = updatedGoal.Status;
        }
    }
} 