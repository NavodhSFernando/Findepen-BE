using FinDepen_Backend.DTOs;

namespace FinDepen_Backend.Repositories
{
    public interface IUserService
    {
        Task<UserBalanceModel> GetUserBalance(string userId);
        Task<UserBalanceModel> SetInitialBalance(string userId, double initialBalance);
        Task<UserBalanceModel> GetMonthlyExpenses(string userId);
    }
} 