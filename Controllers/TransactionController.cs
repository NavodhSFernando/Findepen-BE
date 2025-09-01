using FinDepen_Backend.DTOs;
using FinDepen_Backend.Entities;
using FinDepen_Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinDepen_Backend.Controllers
{
    [Route("api/transactions")]
    [ApiController]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new { Message = "User ID not found in claims" });
                }

                var transactions = await _transactionService.GetTransactions(userId);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving transactions", Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionById(Guid id)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionById(id);
                return Ok(transaction);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = "Transaction not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving the transaction", Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionModel createModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Message = "Invalid model state", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new { Message = "User ID not found in claims" });
                }

                var createdTransaction = await _transactionService.CreateTransaction(createModel, userId);
                return CreatedAtAction(nameof(GetTransactionById), new { id = createdTransaction.Id }, createdTransaction);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while creating the transaction", Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(Guid id, [FromBody] UpdateTransactionModel updateModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Message = "Invalid model state", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new { Message = "User ID not found in claims" });
                }

                var updatedTransaction = await _transactionService.UpdateTransaction(id, updateModel, userId);
                return Ok(updatedTransaction);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the transaction", Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(Guid id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new { Message = "User ID not found in claims" });
                }

                var deletedTransaction = await _transactionService.DeleteTransaction(id, userId);
                return Ok(new { Message = "Transaction deleted successfully", Transaction = deletedTransaction });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting the transaction", Error = ex.Message });
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetTransactionSummary()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new { Message = "User ID not found in claims" });
                }

                var summary = await _transactionService.GetTransactionSummary(userId);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while getting transaction summary", Error = ex.Message });
            }
        }
    }
}
