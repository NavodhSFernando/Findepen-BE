using FinDepen_Backend.DTOs;

namespace FinDepen_Backend.Repositories
{
    public interface IUserService
    {
        Task<UserBalanceModel> GetUserBalance(string userId);
        Task<UserBalanceModel> SetInitialBalance(string userId, double initialBalance);
        Task<UserBalanceModel> GetMonthlyExpenses(string userId);
        
        // New methods for profile management
        Task<UserProfileModel> GetUserProfile(string userId);
        Task<UserProfileModel> UpdateUserProfile(string userId, UpdateProfileModel model);
        Task<bool> ChangePassword(string userId, ChangePasswordModel model);
        Task<UserBalanceModel> AdjustBalance(string userId, BalanceAdjustmentModel model);
        Task<UserSettingsModel> UpdateUserSettings(string userId, UserSettingsModel model);
        Task<UserSettingsModel> GetUserSettings(string userId);
    }
} 