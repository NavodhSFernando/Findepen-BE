using FinDepen_Backend.Entities;

namespace FinDepen_Backend.Repositories
{
    public interface IBudgetService
    {
        Task<IEnumerable<Budget>> GetBudgets(string userId);
        Task<Budget> GetBudgetById(Guid id);
        Task<Budget> CreateBudget(Budget budget);
        Task<Budget> UpdateBudget(Guid id, Budget budget);
        Task<Budget> DeleteBudget(Guid id);
    }
}
