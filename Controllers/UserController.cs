using FinDepen_Backend.DTOs;
using FinDepen_Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinDepen_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "User ID not found in claims" });

            return Ok(new { UserId = userId, Message = "Protected resource accessed!" });
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetUserBalance()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new { Message = "User ID not found in claims" });
                }

                var balance = await _userService.GetUserBalance(userId);
                return Ok(balance);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while getting user balance", Error = ex.Message });
            }
        }

        [HttpPost("balance")]
        public async Task<IActionResult> SetInitialBalance([FromBody] SetInitialBalanceModel model)
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

                var balance = await _userService.SetInitialBalance(userId, model.InitialBalance);
                return Ok(new { Message = "Initial balance set successfully", Balance = balance });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while setting initial balance", Error = ex.Message });
            }
        }

        [HttpGet("monthly-expenses")]
        public async Task<IActionResult> GetMonthlyExpenses()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new { Message = "User ID not found in claims" });
                }

                var monthlyExpenses = await _userService.GetMonthlyExpenses(userId);
                return Ok(monthlyExpenses);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while getting monthly expenses", Error = ex.Message });
            }
        }
    }
}