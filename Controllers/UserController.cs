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
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new { Message = "User ID not found in claims" });
                }

                var profile = await _userService.GetUserProfile(userId);
                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting user profile");
                return StatusCode(500, new { Message = "An error occurred while getting user profile", Error = ex.Message });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileModel model)
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

                var updatedProfile = await _userService.UpdateUserProfile(userId, model);
                return Ok(new { Message = "Profile updated successfully", Profile = updatedProfile });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user profile");
                return StatusCode(500, new { Message = "An error occurred while updating user profile", Error = ex.Message });
            }
        }

        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
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

                var result = await _userService.ChangePassword(userId, model);
                return Ok(new { Message = "Password changed successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while changing password");
                return StatusCode(500, new { Message = "An error occurred while changing password", Error = ex.Message });
            }
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
                _logger.LogError(ex, "An error occurred while getting user balance");
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
                _logger.LogError(ex, "An error occurred while setting initial balance");
                return StatusCode(500, new { Message = "An error occurred while setting initial balance", Error = ex.Message });
            }
        }

        [HttpPut("balance/adjust")]
        public async Task<IActionResult> AdjustBalance([FromBody] BalanceAdjustmentModel model)
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

                var balance = await _userService.AdjustBalance(userId, model);
                return Ok(new { Message = "Balance updated successfully", Balance = balance });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating balance");
                return StatusCode(500, new { Message = "An error occurred while updating balance", Error = ex.Message });
            }
        }

        [HttpGet("settings")]
        public async Task<IActionResult> GetUserSettings()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized(new { Message = "User ID not found in claims" });
                }

                var settings = await _userService.GetUserSettings(userId);
                return Ok(settings);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting user settings");
                return StatusCode(500, new { Message = "An error occurred while getting user settings", Error = ex.Message });
            }
        }

        [HttpPut("settings")]
        public async Task<IActionResult> UpdateUserSettings([FromBody] UserSettingsModel model)
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

                var settings = await _userService.UpdateUserSettings(userId, model);
                return Ok(new { Message = "Settings updated successfully", Settings = settings });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user settings");
                return StatusCode(500, new { Message = "An error occurred while updating user settings", Error = ex.Message });
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
                _logger.LogError(ex, "An error occurred while getting monthly expenses");
                return StatusCode(500, new { Message = "An error occurred while getting monthly expenses", Error = ex.Message });
            }
        }
    }
}