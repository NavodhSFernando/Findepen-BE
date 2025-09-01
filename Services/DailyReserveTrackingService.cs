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
    public class DailyReserveTrackingService : BackgroundService, IDailyReserveTrackingService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyReserveTrackingService> _logger;
        private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24); // Run daily

        public DailyReserveTrackingService(IServiceProvider serviceProvider, ILogger<DailyReserveTrackingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily reserve tracking service started");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessDailyReserveSnapshots(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during daily reserve tracking process.");
                }

                await Task.Delay(RunInterval, stoppingToken);
            }
        }

        private async Task ProcessDailyReserveSnapshots(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var today = DateTime.UtcNow.Date;
            _logger.LogInformation("Processing daily reserve snapshots for date: {Date}", today);

            // Get all users who have goals
            var usersWithGoals = await dbContext.Users
                .Where(u => u.Goals.Any())
                .ToListAsync(stoppingToken);

            if (!usersWithGoals.Any())
            {
                _logger.LogDebug("No users with goals found for date: {Date}", today);
                return;
            }

            _logger.LogInformation("Found {Count} users with goals to process", usersWithGoals.Count);

            foreach (var user in usersWithGoals)
            {
                try
                {
                    await CreateDailyReserveSnapshot(user.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create reserve snapshot for user {UserId}", user.Id);
                }
            }

            _logger.LogInformation("Completed processing daily reserve snapshots for {Count} users", usersWithGoals.Count);
        }

        public async Task<DailyReserveSnapshot> CreateDailyReserveSnapshot(string userId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var today = DateTime.UtcNow.Date;

                // Check if snapshot already exists for today
                if (await SnapshotExistsForDate(userId, today))
                {
                    _logger.LogDebug("Reserve snapshot already exists for user {UserId} on date {Date}", userId, today);
                    return await GetLatestReserveSnapshot(userId) ?? throw new InvalidOperationException("Snapshot not found after existence check");
                }

                // Calculate total reserve amount from active goals
                var totalReserveAmount = await CalculateTotalReserveAmount(dbContext, userId);

                // Create new snapshot
                var snapshot = new DailyReserveSnapshot
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Date = today,
                    ReserveAmount = totalReserveAmount,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.DailyReserveSnapshots.Add(snapshot);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation("Successfully created reserve snapshot for user {UserId} with amount {Amount} on date {Date}",
                    userId, totalReserveAmount, today);

                return snapshot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating reserve snapshot for user: {UserId}", userId);
                throw new Exception("An error occurred while creating the reserve snapshot.", ex);
            }
        }

        private async Task<double> CalculateTotalReserveAmount(ApplicationDbContext dbContext, string userId)
        {
            // Get all active goals for the user and sum their current amounts
            var activeGoals = await dbContext.Goals
                .Where(g => g.UserId == userId && g.Status == Constants.GoalStatus.Active)
                .ToListAsync();

            var totalReserveAmount = activeGoals.Sum(g => g.CurrentAmount);
            return totalReserveAmount;
        }

        public async Task<IEnumerable<DailyReserveSnapshot>> GetDailyReserveHistory(string userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                _logger.LogInformation("Retrieving reserve history for user {UserId} from {StartDate} to {EndDate}", 
                    userId, startDate, endDate);

                var snapshots = await dbContext.DailyReserveSnapshots
                    .Where(s => s.UserId == userId && s.Date >= startDate && s.Date <= endDate)
                    .OrderBy(s => s.Date)
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {Count} reserve snapshots for user {UserId}", 
                    snapshots.Count, userId);

                return snapshots;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving reserve history for user: {UserId}", userId);
                throw new Exception("An error occurred while retrieving the reserve history.", ex);
            }
        }

        public async Task<DailyReserveSnapshot?> GetLatestReserveSnapshot(string userId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var latestSnapshot = await dbContext.DailyReserveSnapshots
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.Date)
                    .FirstOrDefaultAsync();

                return latestSnapshot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving latest reserve snapshot for user: {UserId}", userId);
                throw new Exception("An error occurred while retrieving the latest reserve snapshot.", ex);
            }
        }

        public async Task<bool> SnapshotExistsForDate(string userId, DateTime date)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                return await dbContext.DailyReserveSnapshots
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
                await CreateDailyReserveSnapshot(userId);
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
