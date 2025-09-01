using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FinDepen_Backend.Data;
using FinDepen_Backend.Entities;
using FinDepen_Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinDepen_Backend.Services
{
    public class DailyBalanceTrackingService : BackgroundService, IDailyBalanceTrackingService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyBalanceTrackingService> _logger;
        private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24); // Run daily

        public DailyBalanceTrackingService(IServiceProvider serviceProvider, ILogger<DailyBalanceTrackingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily balance tracking service started");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessDailyBalanceSnapshots(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during daily balance tracking process.");
                }

                await Task.Delay(RunInterval, stoppingToken);
            }
        }

        private async Task ProcessDailyBalanceSnapshots(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var today = DateTime.UtcNow.Date;
            _logger.LogInformation("Processing daily balance snapshots for date: {Date}", today);

            // Get all users with balance amounts
            var usersWithBalance = await dbContext.Users
                .Where(u => u.BalanceAmount.HasValue)
                .ToListAsync(stoppingToken);

            if (!usersWithBalance.Any())
            {
                _logger.LogDebug("No users with balance amounts found for date: {Date}", today);
                return;
            }

            _logger.LogInformation("Found {Count} users with balance amounts to process", usersWithBalance.Count);

            foreach (var user in usersWithBalance)
            {
                try
                {
                    await CreateDailyBalanceSnapshot(user.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create balance snapshot for user {UserId}", user.Id);
                }
            }

            _logger.LogInformation("Completed processing daily balance snapshots for {Count} users", usersWithBalance.Count);
        }

        public async Task<DailyBalanceSnapshot> CreateDailyBalanceSnapshot(string userId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var today = DateTime.UtcNow.Date;

                // Check if snapshot already exists for today
                if (await SnapshotExistsForDate(userId, today))
                {
                    _logger.LogDebug("Balance snapshot already exists for user {UserId} on date {Date}", userId, today);
                    return await GetLatestBalanceSnapshot(userId) ?? throw new InvalidOperationException("Snapshot not found after existence check");
                }

                // Get current user balance
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                }

                var balanceAmount = user.BalanceAmount ?? 0.0;

                // Create new snapshot
                var snapshot = new DailyBalanceSnapshot
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Date = today,
                    BalanceAmount = balanceAmount,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.DailyBalanceSnapshots.Add(snapshot);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation("Successfully created balance snapshot for user {UserId} with amount {Amount} on date {Date}",
                    userId, balanceAmount, today);

                return snapshot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating balance snapshot for user: {UserId}", userId);
                throw new Exception("An error occurred while creating the balance snapshot.", ex);
            }
        }

        public async Task<IEnumerable<DailyBalanceSnapshot>> GetDailyBalanceHistory(string userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                _logger.LogInformation("Retrieving balance history for user {UserId} from {StartDate} to {EndDate}", 
                    userId, startDate, endDate);

                var snapshots = await dbContext.DailyBalanceSnapshots
                    .Where(s => s.UserId == userId && s.Date >= startDate && s.Date <= endDate)
                    .OrderBy(s => s.Date)
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {Count} balance snapshots for user {UserId}", 
                    snapshots.Count, userId);

                return snapshots;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving balance history for user: {UserId}", userId);
                throw new Exception("An error occurred while retrieving the balance history.", ex);
            }
        }

        public async Task<DailyBalanceSnapshot?> GetLatestBalanceSnapshot(string userId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var latestSnapshot = await dbContext.DailyBalanceSnapshots
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.Date)
                    .FirstOrDefaultAsync();

                return latestSnapshot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving latest balance snapshot for user: {UserId}", userId);
                throw new Exception("An error occurred while retrieving the latest balance snapshot.", ex);
            }
        }

        public async Task<bool> SnapshotExistsForDate(string userId, DateTime date)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                return await dbContext.DailyBalanceSnapshots
                    .AnyAsync(s => s.UserId == userId && s.Date == date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking snapshot existence for user: {UserId} on date: {Date}", userId, date);
                throw new Exception("An error occurred while checking snapshot existence.", ex);
            }
        }

        // Method for testing purposes - manually trigger snapshot creation
        public async Task<bool> TestSnapshotCreation(string userId, CancellationToken stoppingToken = default)
        {
            try
            {
                await CreateDailyBalanceSnapshot(userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test snapshot creation failed for user {UserId}", userId);
                return false;
            }
        }
    }
}
