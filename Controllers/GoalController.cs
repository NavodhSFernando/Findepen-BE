using FinDepen_Backend.DTOs;
using FinDepen_Backend.Entities;
using FinDepen_Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinDepen_Backend.Controllers
{
    [Route("api/goals")]
    [ApiController]
    [Authorize]
    public class GoalController : ControllerBase
    {
        private readonly IGoalService _goalService;
        private readonly ILogger<GoalController> _logger;

        public GoalController(IGoalService goalService, ILogger<GoalController> logger)
        {
            _goalService = goalService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetGoals()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var goals = await _goalService.GetGoals(userId);
                return Ok(goals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving goals for user");
                return StatusCode(500, "An error occurred while retrieving goals");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGoalById(Guid id)
        {
            try
            {
                var goal = await _goalService.GetGoalById(id);
                return Ok(goal);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Goal with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving goal with ID: {GoalId}", id);
                return StatusCode(500, "An error occurred while retrieving the goal");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateGoal([FromBody] CreateGoalModel createGoalModel)
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

                var goal = new Goal
                {
                    Title = createGoalModel.Title,
                    Description = createGoalModel.Description,
                    TargetAmount = createGoalModel.TargetAmount,
                    CurrentAmount = 0, // Initialize to 0 for new goals
                    TargetDate = createGoalModel.TargetDate,
                    Priority = createGoalModel.Priority,
                    Reminder = createGoalModel.Reminder,
                    UserId = userId
                };

                var createdGoal = await _goalService.CreateGoal(goal);
                return CreatedAtAction(nameof(GetGoalById), new { id = createdGoal.Id }, createdGoal);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating goal for user");
                return StatusCode(500, "An error occurred while creating the goal");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGoal(Guid id, [FromBody] UpdateGoalModel updateGoalModel)
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

                var updatedGoal = new Goal
                {
                    Title = updateGoalModel.Title,
                    Description = updateGoalModel.Description,
                    TargetAmount = updateGoalModel.TargetAmount,
                    CurrentAmount = updateGoalModel.CurrentAmount,
                    TargetDate = updateGoalModel.TargetDate,
                    Priority = updateGoalModel.Priority,
                    IsActive = updateGoalModel.IsActive,
                    Reminder = updateGoalModel.Reminder,
                    Status = updateGoalModel.Status,
                    UserId = userId
                };

                var result = await _goalService.UpdateGoal(id, updatedGoal);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Goal with ID {id} not found");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating goal with ID: {GoalId}", id);
                return StatusCode(500, "An error occurred while updating the goal");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGoal(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var deletedGoal = await _goalService.DeleteGoal(id);
                return Ok(deletedGoal);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Goal with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting goal with ID: {GoalId}", id);
                return StatusCode(500, "An error occurred while deleting the goal");
            }
        }

        [HttpPost("{id}/add-funds")]
        public async Task<IActionResult> AddFundsToGoal(Guid id, [FromBody] AddFundsToGoalModel model)
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

                var updatedGoal = await _goalService.AddFundsToGoal(id, model.Amount, userId);
                return Ok(updatedGoal);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Goal with ID {id} not found");
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("You can only modify your own goals");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding funds to goal with ID: {GoalId}", id);
                return StatusCode(500, "An error occurred while adding funds to the goal");
            }
        }

        [HttpPost("{id}/withdraw-funds")]
        public async Task<IActionResult> WithdrawFundsFromGoal(Guid id, [FromBody] WithdrawFundsFromGoalModel model)
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

                var updatedGoal = await _goalService.WithdrawFundsFromGoal(id, model.Amount, userId);
                return Ok(updatedGoal);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Goal with ID {id} not found");
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("You can only modify your own goals");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error withdrawing funds from goal with ID: {GoalId}", id);
                return StatusCode(500, "An error occurred while withdrawing funds from the goal");
            }
        }

        [HttpPost("{id}/convert-to-expense")]
        public async Task<IActionResult> ConvertGoalToExpense(Guid id, [FromBody] ConvertGoalToExpenseModel model)
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

                var updatedGoal = await _goalService.ConvertGoalToExpense(
                    id, 
                    model.Amount, 
                    model.TransactionTitle, 
                    model.TransactionDescription, 
                    model.Category, 
                    userId);
                
                return Ok(updatedGoal);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Goal with ID {id} not found");
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("You can only modify your own goals");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting goal with ID: {GoalId} to expense", id);
                return StatusCode(500, "An error occurred while converting the goal to expense");
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetGoalSummary()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var summary = await _goalService.GetGoalSummary(userId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving goal summary for user");
                return StatusCode(500, "An error occurred while retrieving the goal summary");
            }
        }

        private string? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim?.Value;
        }
    }
} 