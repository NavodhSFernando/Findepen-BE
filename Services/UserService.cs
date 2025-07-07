using AutoMapper;
using FinDepen_Backend.Data;
using FinDepen_Backend.DTOs;
using FinDepen_Backend.Entities;
using FinDepen_Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinDepen_Backend.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, IMapper mapper, ILogger<UserService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
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
    }
} 