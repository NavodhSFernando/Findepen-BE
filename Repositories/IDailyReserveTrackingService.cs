using FinDepen_Backend.Entities;

namespace FinDepen_Backend.Repositories
{
    public interface IDailyReserveTrackingService
    {
        Task<DailyReserveSnapshot> CreateDailyReserveSnapshot(string userId);
        Task<IEnumerable<DailyReserveSnapshot>> GetDailyReserveHistory(string userId, DateTime startDate, DateTime endDate);
        Task<DailyReserveSnapshot?> GetLatestReserveSnapshot(string userId);
        Task<bool> SnapshotExistsForDate(string userId, DateTime date);
    }
}
