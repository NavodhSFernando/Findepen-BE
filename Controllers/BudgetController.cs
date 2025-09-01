using FinDepen_Backend.Constants;
using FinDepen_Backend.DTOs;
using FinDepen_Backend.Entities;
using FinDepen_Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinDepen_Backend.Controllers
{
    [Route("api/budgets")]
    [ApiController]
    [Authorize]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetService _budgetService;
        private readonly ILogger<BudgetController> _logger;

        public BudgetController(IBudgetService budgetService, ILogger<BudgetController> logger)
        {
            _budgetService = budgetService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetBudgets()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var budgets = await _budgetService.GetBudgets(userId);
                return Ok(budgets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budgets for user");
                return StatusCode(500, "An error occurred while retrieving budgets");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBudgetById(Guid id)
        {
            try
            {
                var budget = await _budgetService.GetBudgetById(id);
                return Ok(budget);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Budget with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget with ID: {BudgetId}", id);
                return StatusCode(500, "An error occurred while retrieving the budget");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetModel createBudgetModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var budget = new Budget
                {
                    Category = createBudgetModel.Category,
                    PlannedAmount = createBudgetModel.PlannedAmount,
                    SpentAmount = 0, // Initialize to 0 for new budgets
                    Reminder = createBudgetModel.Reminder,
                    StartDate = createBudgetModel.StartDate,
                    RenewalFrequency = createBudgetModel.RenewalFrequency,
                    AutoRenewalEnabled = createBudgetModel.AutoRenewalEnabled,
                    UserId = userId
                };

                var createdBudget = await _budgetService.CreateBudget(budget);
                return CreatedAtAction(nameof(GetBudgetById), new { id = createdBudget.Id }, createdBudget);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating budget for user");
                return StatusCode(500, "An error occurred while creating the budget");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBudget(Guid id, [FromBody] UpdateBudgetModel updateBudgetModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var updatedBudget = new Budget
                {
                    Category = updateBudgetModel.Category,
                    PlannedAmount = updateBudgetModel.PlannedAmount,
                    Reminder = updateBudgetModel.Reminder,
                    StartDate = updateBudgetModel.StartDate,
                    RenewalFrequency = updateBudgetModel.RenewalFrequency,
                    AutoRenewalEnabled = updateBudgetModel.AutoRenewalEnabled,
                    UserId = userId
                };

                var result = await _budgetService.UpdateBudget(id, updatedBudget);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Budget with ID {id} not found");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating budget with ID: {BudgetId}", id);
                return StatusCode(500, "An error occurred while updating the budget");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudget(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var deletedBudget = await _budgetService.DeleteBudget(id);
                return Ok(deletedBudget);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Budget with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting budget with ID: {BudgetId}", id);
                return StatusCode(500, "An error occurred while deleting the budget");
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetBudgetSummary()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var budgets = await _budgetService.GetBudgets(userId);
                var budgetList = budgets.ToList();

                _logger.LogInformation("Calculating budget summary for user {UserId} based on {Count} ongoing budgets", 
                    userId, budgetList.Count);

                var summary = new BudgetSummaryModel
                {
                    TotalBudgets = budgetList.Count,
                    TotalPlannedAmount = budgetList.Sum(b => b.PlannedAmount),
                    TotalSpentAmount = budgetList.Sum(b => b.SpentAmount),
                    TotalRemainingAmount = budgetList.Sum(b => b.PlannedAmount - b.SpentAmount),
                    OverallProgressPercentage = budgetList.Any() ? 
                        (budgetList.Sum(b => b.SpentAmount) / budgetList.Sum(b => b.PlannedAmount)) * 100 : 0,
                    OnTrackBudgets = budgetList.Count(b => b.SpentAmount < b.PlannedAmount * 0.8),
                    WarningBudgets = budgetList.Count(b => b.SpentAmount >= b.PlannedAmount * 0.8 && b.SpentAmount < b.PlannedAmount),
                    ExceededBudgets = budgetList.Count(b => b.SpentAmount >= b.PlannedAmount)
                };

                _logger.LogInformation("Budget summary calculated for user {UserId}: Total={Total}, Planned={Planned}, Spent={Spent}, Progress={Progress}%", 
                    userId, summary.TotalBudgets, summary.TotalPlannedAmount, summary.TotalSpentAmount, summary.OverallProgressPercentage);

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget summary for user");
                return StatusCode(500, "An error occurred while retrieving budget summary");
            }
        }

        [HttpGet("categories-with-active-budgets")]
        public async Task<IActionResult> GetCategoriesWithActiveBudgets()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var categories = await _budgetService.GetCategoriesWithActiveBudgets(userId);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories with active budgets for user");
                return StatusCode(500, "An error occurred while retrieving categories with active budgets");
            }
        }

        [HttpPut("{id}/auto-renewal")]
        public async Task<IActionResult> ToggleAutoRenewal(Guid id, [FromBody] ToggleAutoRenewalModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var budget = await _budgetService.GetBudgetById(id);
                
                // Ensure the budget belongs to the current user
                if (budget.UserId != userId)
                {
                    return Forbid("You can only modify your own budgets");
                }

                budget.AutoRenewalEnabled = model.AutoRenewalEnabled;
                
                var updatedBudget = await _budgetService.UpdateBudget(id, budget);
                
                _logger.LogInformation("Auto-renewal toggled for budget {BudgetId} to {AutoRenewalEnabled} by user {UserId}", 
                    id, model.AutoRenewalEnabled, userId);
                
                return Ok(new { 
                    Message = $"Auto-renewal {(model.AutoRenewalEnabled ? "enabled" : "disabled")} successfully",
                    Budget = updatedBudget 
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Budget with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling auto-renewal for budget with ID: {BudgetId}", id);
                return StatusCode(500, "An error occurred while toggling auto-renewal");
            }
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
} 