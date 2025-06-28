using AutoMapper;
using FinDepen_Backend.Data;
using FinDepen_Backend.Entities;
using FinDepen_Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinDepen_Backend.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<BudgetService> _logger;
        public BudgetService(ApplicationDbContext context, IMapper mapper, ILogger<BudgetService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<Budget>> GetBudgets(string userId)
        {
            try
            {
                return await _context.Budgets
                .Where(b => b.UserId == userId)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving transactions for user: {UserId}", userId);
                throw new Exception("An error occurred while retrieving transactions.", ex);
            }
        }
        public Task<Budget> GetBudgetById(Guid id)
        {
            throw new NotImplementedException();
        }
        public Task<Budget> CreateBudget(Budget budget)
        {
            throw new NotImplementedException();
        }
        public Task<Budget> UpdateBudget(Guid id, Budget budget)
        {
            throw new NotImplementedException();
        }
        public Task<Budget> DeleteBudget(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
