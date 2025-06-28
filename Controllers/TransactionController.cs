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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var transactions = await _transactionService.GetTransactions(userId);
            return Ok(transactions);
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
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction(TransactionModel transactionModel)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var transaction = new Transaction
            {
                Title = transactionModel.Title,
                Amount = transactionModel.Amount,
                Category = transactionModel.Category,
                Date = transactionModel.Date,
                Description = transactionModel.Description,
                Type = transactionModel.Type,
                UserId = userId
            };

            var createdTransaction = await _transactionService.CreateTransaction(transaction);
            return Ok(createdTransaction);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(Guid id, TransactionModel transactionModel)
        {
            try
            {
                var updatedTransaction = await _transactionService.UpdateTransaction(id, transactionModel);
                return Ok(updatedTransaction);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            try
            {
                var deletedTransaction = await _transactionService.DeleteTransaction(id);
                return Ok(deletedTransaction);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
