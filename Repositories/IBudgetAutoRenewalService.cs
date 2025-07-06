namespace FinDepen_Backend.Services
{
    public interface IBudgetAutoRenewalService
    {
        Task<bool> TestRenewalForBudget(Guid budgetId, CancellationToken stoppingToken = default);
    }
} 