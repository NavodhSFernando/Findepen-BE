using AutoMapper;
using FinDepen_Backend.Data;
using FinDepen_Backend.DTOs;
using FinDepen_Backend.Entities;
using FinDepen_Backend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinDepen_Backend.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(ApplicationDbContext context, IMapper mapper, ILogger<UserService> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<UserBalanceModel> GetUserBalance(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                var currentMonth = DateTime.UtcNow;
                var startOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var monthlyTransactions = await _context.Transactions
                    .Where(t => t.UserId == userId && t.Date >= startOfMonth && t.Date <= endOfMonth)
                    .ToListAsync();

                var balanceModel = new UserBalanceModel
                {
                    CurrentBalance = user.BalanceAmount ?? 0,
                    MonthlyIncome = monthlyTransactions.Where(t => t.Type == "Income").Sum(t => t.Amount),
                    MonthlyExpenses = monthlyTransactions.Where(t => t.Type == "Expense").Sum(t => t.Amount)
                };

                balanceModel.MonthlyNet = balanceModel.MonthlyIncome - balanceModel.MonthlyExpenses;

                return balanceModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting user balance for user: {UserId}", userId);
                throw new Exception("An error occurred while getting user balance.", ex);
            }
        }

        public async Task<UserBalanceModel> SetInitialBalance(string userId, double initialBalance)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                user.BalanceAmount = initialBalance;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Initial balance set successfully for user: {UserId}, Balance: {Balance}", userId, initialBalance);

                return new UserBalanceModel
                {
                    CurrentBalance = initialBalance,
                    MonthlyIncome = 0,
                    MonthlyExpenses = 0,
                    MonthlyNet = 0
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while setting initial balance for user: {UserId}", userId);
                throw new Exception("An error occurred while setting initial balance.", ex);
            }
        }

        public async Task<UserBalanceModel> GetMonthlyExpenses(string userId)
        {
            try
            {
                var currentMonth = DateTime.UtcNow;
                var startOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var monthlyTransactions = await _context.Transactions
                    .Where(t => t.UserId == userId && t.Date >= startOfMonth && t.Date <= endOfMonth)
                    .ToListAsync();

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                var balanceModel = new UserBalanceModel
                {
                    CurrentBalance = user.BalanceAmount ?? 0,
                    MonthlyIncome = monthlyTransactions.Where(t => t.Type == "Income").Sum(t => t.Amount),
                    MonthlyExpenses = monthlyTransactions.Where(t => t.Type == "Expense").Sum(t => t.Amount)
                };

                balanceModel.MonthlyNet = balanceModel.MonthlyIncome - balanceModel.MonthlyExpenses;

                return balanceModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting monthly expenses for user: {UserId}", userId);
                throw new Exception("An error occurred while getting monthly expenses.", ex);
            }
        }

        public async Task<UserProfileModel> GetUserProfile(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                var profileModel = new UserProfileModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    DOB = user.DOB,
                    BalanceAmount = user.BalanceAmount,
                    Theme = user.Theme
                };

                _logger.LogInformation("Successfully retrieved profile for user: {UserId}", userId);
                return profileModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting user profile for user: {UserId}", userId);
                throw new Exception("An error occurred while getting user profile.", ex);
            }
        }

        public async Task<UserProfileModel> UpdateUserProfile(string userId, UpdateProfileModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                // Check if email is being changed and if it's already in use by another user
                if (model.Email != user.Email)
                {
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Id != userId);
                    if (existingUser != null)
                    {
                        throw new ArgumentException("Email is already in use by another user");
                    }
                }

                // Update user properties
                user.Name = model.Name;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.DOB = model.DOB;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Profile updated successfully for user: {UserId}", userId);

                return new UserProfileModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    DOB = user.DOB,
                    BalanceAmount = user.BalanceAmount,
                    Theme = user.Theme
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while updating user profile for user: {UserId}", userId);
                throw new Exception("An error occurred while updating user profile.", ex);
            }
        }

        public async Task<bool> ChangePassword(string userId, ChangePasswordModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new ArgumentException($"Password change failed: {errors}");
                }

                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while changing password for user: {UserId}", userId);
                throw new Exception("An error occurred while changing password.", ex);
            }
        }

        public async Task<UserBalanceModel> AdjustBalance(string userId, BalanceAdjustmentModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                // Replace current balance with new amount
                var newBalance = model.Amount;

                // Prevent negative balance
                if (newBalance < 0)
                {
                    throw new ArgumentException("Balance cannot be negative");
                }

                user.BalanceAmount = newBalance;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Balance updated successfully for user: {UserId}, New Balance: {NewBalance}, Reason: {Reason}", 
                    userId, newBalance, model.Reason);

                return new UserBalanceModel
                {
                    CurrentBalance = newBalance,
                    MonthlyIncome = 0,
                    MonthlyExpenses = 0,
                    MonthlyNet = 0
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while updating balance for user: {UserId}", userId);
                throw new Exception("An error occurred while updating balance.", ex);
            }
        }

        public async Task<UserSettingsModel> UpdateUserSettings(string userId, UserSettingsModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                user.Theme = model.Theme;
                // Note: We don't have UpdatedAt property in ApplicationUser

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Settings updated successfully for user: {UserId}, Theme: {Theme}", userId, model.Theme);

                return new UserSettingsModel
                {
                    Theme = user.Theme
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while updating user settings for user: {UserId}", userId);
                throw new Exception("An error occurred while updating user settings.", ex);
            }
        }

        public async Task<UserSettingsModel> GetUserSettings(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found");
                }

                var settingsModel = new UserSettingsModel
                {
                    Theme = user.Theme
                };

                _logger.LogInformation("Successfully retrieved settings for user: {UserId}", userId);
                return settingsModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting user settings for user: {UserId}", userId);
                throw new Exception("An error occurred while getting user settings.", ex);
            }
        }
    }
} 