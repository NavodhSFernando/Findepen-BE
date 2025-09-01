using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinDepen_Backend.DTOs;
using FinDepen_Backend.Services;
using System.Security.Claims;

namespace FinDepen_Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RecurringTransactionController : ControllerBase
    {
        private readonly IRecurringTransactionService _recurringTransactionService;
        private readonly ILogger<RecurringTransactionController> _logger;

        public RecurringTransactionController(IRecurringTransactionService recurringTransactionService, ILogger<RecurringTransactionController> logger)
        {
            _recurringTransactionService = recurringTransactionService;
            _logger = logger;
        }

        /// <summary>
        /// Get all recurring transactions for the authenticated user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RecurringTransactionModel>>> GetRecurringTransactions()
        {
            try
            {
                var userId = GetUserId();
                var recurringTransactions = await _recurringTransactionService.GetRecurringTransactions(userId);
                return Ok(recurringTransactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recurring transactions for user");
                return StatusCode(500, "An error occurred while retrieving recurring transactions");
            }
        }

        /// <summary>
        /// Get a specific recurring transaction by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RecurringTransactionModel>> GetRecurringTransaction(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var recurringTransaction = await _recurringTransactionService.GetRecurringTransactionById(id, userId);
                return Ok(recurringTransaction);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recurring transaction not found with id: {Id}", id);
                return NotFound("Recurring transaction not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recurring transaction with id: {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the recurring transaction");
            }
        }

        /// <summary>
        /// Create a new recurring transaction
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RecurringTransactionModel>> CreateRecurringTransaction([FromBody] CreateRecurringTransactionModel createModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetUserId();
                var recurringTransaction = await _recurringTransactionService.CreateRecurringTransaction(createModel, userId);
                return CreatedAtAction(nameof(GetRecurringTransaction), new { id = recurringTransaction.Id }, recurringTransaction);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid data provided for creating recurring transaction");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recurring transaction");
                return StatusCode(500, "An error occurred while creating the recurring transaction");
            }
        }

        /// <summary>
        /// Update an existing recurring transaction
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<RecurringTransactionModel>> UpdateRecurringTransaction(Guid id, [FromBody] UpdateRecurringTransactionModel updateModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetUserId();
                var recurringTransaction = await _recurringTransactionService.UpdateRecurringTransaction(id, updateModel, userId);
                return Ok(recurringTransaction);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recurring transaction not found with id: {Id}", id);
                return NotFound("Recurring transaction not found");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid data provided for updating recurring transaction with id: {Id}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recurring transaction with id: {Id}", id);
                return StatusCode(500, "An error occurred while updating the recurring transaction");
            }
        }

        /// <summary>
        /// Update the status of a recurring transaction
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<RecurringTransactionModel>> UpdateRecurringTransactionStatus(Guid id, [FromBody] UpdateRecurringTransactionStatusModel statusModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetUserId();
                var recurringTransaction = await _recurringTransactionService.UpdateRecurringTransactionStatus(id, statusModel, userId);
                return Ok(recurringTransaction);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Recurring transaction not found with id: {Id}", id);
                return NotFound("Recurring transaction not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status of recurring transaction with id: {Id}", id);
                return StatusCode(500, "An error occurred while updating the recurring transaction status");
            }
        }

        /// <summary>
        /// Delete a recurring transaction
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRecurringTransaction(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var deleted = await _recurringTransactionService.DeleteRecurringTransaction(id, userId);
                
                if (!deleted)
                {
                    return NotFound("Recurring transaction not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recurring transaction with id: {Id}", id);
                return StatusCode(500, "An error occurred while deleting the recurring transaction");
            }
        }

        /// <summary>
        /// Get recurring transaction summary and analytics
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<RecurringTransactionSummaryModel>> GetRecurringTransactionSummary()
        {
            try
            {
                var userId = GetUserId();
                var summary = await _recurringTransactionService.GetRecurringTransactionSummary(userId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recurring transaction summary");
                return StatusCode(500, "An error occurred while retrieving the recurring transaction summary");
            }
        }

        /// <summary>
        /// Get recurring transactions by status
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<RecurringTransactionModel>>> GetRecurringTransactionsByStatus(string status)
        {
            try
            {
                var userId = GetUserId();
                var recurringTransactions = await _recurringTransactionService.GetRecurringTransactionsByStatus(userId, status);
                return Ok(recurringTransactions);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid status provided: {Status}", status);
                return BadRequest("Invalid status value");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recurring transactions by status: {Status}", status);
                return StatusCode(500, "An error occurred while retrieving recurring transactions by status");
            }
        }

        /// <summary>
        /// Get recurring transactions by frequency
        /// </summary>
        [HttpGet("frequency/{frequency}")]
        public async Task<ActionResult<IEnumerable<RecurringTransactionModel>>> GetRecurringTransactionsByFrequency(string frequency)
        {
            try
            {
                var userId = GetUserId();
                var recurringTransactions = await _recurringTransactionService.GetRecurringTransactionsByFrequency(userId, frequency);
                return Ok(recurringTransactions);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid frequency provided: {Frequency}", frequency);
                return BadRequest("Invalid frequency value");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recurring transactions by frequency: {Frequency}", frequency);
                return StatusCode(500, "An error occurred while retrieving recurring transactions by frequency");
            }
        }

        /// <summary>
        /// Pause a recurring transaction
        /// </summary>
        [HttpPost("{id}/pause")]
        public async Task<ActionResult> PauseRecurringTransaction(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var paused = await _recurringTransactionService.PauseRecurringTransaction(id, userId);
                
                if (!paused)
                {
                    return NotFound("Recurring transaction not found");
                }

                return Ok(new { message = "Recurring transaction paused successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing recurring transaction with id: {Id}", id);
                return StatusCode(500, "An error occurred while pausing the recurring transaction");
            }
        }

        /// <summary>
        /// Resume a paused recurring transaction
        /// </summary>
        [HttpPost("{id}/resume")]
        public async Task<ActionResult> ResumeRecurringTransaction(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var resumed = await _recurringTransactionService.ResumeRecurringTransaction(id, userId);
                
                if (!resumed)
                {
                    return NotFound("Recurring transaction not found");
                }

                return Ok(new { message = "Recurring transaction resumed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming recurring transaction with id: {Id}", id);
                return StatusCode(500, "An error occurred while resuming the recurring transaction");
            }
        }

        /// <summary>
        /// Cancel a recurring transaction
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> CancelRecurringTransaction(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var cancelled = await _recurringTransactionService.CancelRecurringTransaction(id, userId);
                
                if (!cancelled)
                {
                    return NotFound("Recurring transaction not found");
                }

                return Ok(new { message = "Recurring transaction cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling recurring transaction with id: {Id}", id);
                return StatusCode(500, "An error occurred while cancelling the recurring transaction");
            }
        }

        /// <summary>
        /// Calculate next occurrence date for a given frequency
        /// </summary>
        [HttpPost("calculate-next-occurrence")]
        public async Task<ActionResult<DateTime>> CalculateNextOccurrenceDate([FromBody] CalculateNextOccurrenceRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var nextOccurrenceDate = await _recurringTransactionService.CalculateNextOccurrenceDate(request.CurrentDate, request.Frequency);
                return Ok(nextOccurrenceDate);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid frequency provided: {Frequency}", request.Frequency);
                return BadRequest("Invalid frequency value");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next occurrence date");
                return StatusCode(500, "An error occurred while calculating the next occurrence date");
            }
        }

        private string GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found in claims");
            }
            return userId;
        }
    }

    public class CalculateNextOccurrenceRequest
    {
        public DateTime CurrentDate { get; set; }
        public string Frequency { get; set; } = string.Empty;
    }
}
