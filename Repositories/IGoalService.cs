using FinDepen_Backend.Entities;
using FinDepen_Backend.DTOs;

namespace FinDepen_Backend.Repositories
{
    public interface IGoalService
    {
        Task<IEnumerable<Goal>> GetGoals(string userId);
        Task<Goal> GetGoalById(Guid id);
        Task<Goal> CreateGoal(Goal goal);
        Task<Goal> UpdateGoal(Guid id, Goal updatedGoal);
        Task<Goal> DeleteGoal(Guid id);
        Task<Goal> AddFundsToGoal(Guid goalId, double amount, string userId);
        Task<Goal> WithdrawFundsFromGoal(Guid goalId, double amount, string userId);
        Task<Goal> ConvertGoalToExpense(Guid goalId, double amount, string transactionTitle, string? transactionDescription, string category, string userId);
        Task<GoalSummaryModel> GetGoalSummary(string userId);
    }
} 