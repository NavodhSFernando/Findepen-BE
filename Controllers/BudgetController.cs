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
                    SpentAmount = updateBudgetModel.SpentAmount,
                    Reminder = updateBudgetModel.Reminder,
                    StartDate = updateBudgetModel.StartDate,
                    RenewalFrequency = updateBudgetModel.RenewalFrequency,
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

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving budget summary for user");
                return StatusCode(500, "An error occurred while retrieving budget summary");
            }
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
} 