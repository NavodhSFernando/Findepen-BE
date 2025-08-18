using FinDepen_Backend.Entities;

namespace FinDepen_Backend.Repositories
{
    public interface IDailyBalanceTrackingService
    {
        Task<DailyBalanceSnapshot> CreateDailyBalanceSnapshot(string userId);
        Task<IEnumerable<DailyBalanceSnapshot>> GetDailyBalanceHistory(string userId, DateTime startDate, DateTime endDate);
        Task<DailyBalanceSnapshot?> GetLatestBalanceSnapshot(string userId);
        Task<bool> SnapshotExistsForDate(string userId, DateTime date);
    }
}
