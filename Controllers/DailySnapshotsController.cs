using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinDepen_Backend.Repositories;
using FinDepen_Backend.Entities;
using System.Security.Claims;

namespace FinDepen_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DailySnapshotsController : ControllerBase
    {
        private readonly IDailyBalanceTrackingService _balanceTrackingService;
        private readonly IDailyReserveTrackingService _reserveTrackingService;
        private readonly ILogger<DailySnapshotsController> _logger;

        public DailySnapshotsController(
            IDailyBalanceTrackingService balanceTrackingService,
            IDailyReserveTrackingService reserveTrackingService,
            ILogger<DailySnapshotsController> logger)
        {
            _balanceTrackingService = balanceTrackingService;
            _reserveTrackingService = reserveTrackingService;
            _logger = logger;
        }

        /// <summary>
        /// Get daily balance history for the authenticated user
        /// </summary>
        /// <param name="startDate">Start date for the range (optional, defaults to 30 days ago)</param>
        /// <param name="endDate">End date for the range (optional, defaults to today)</param>
        /// <returns>List of daily balance snapshots</returns>
        [HttpGet("balance")]
        public async Task<ActionResult<IEnumerable<DailyBalanceSnapshot>>> GetBalanceHistory(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                if (start > end)
                {
                    return BadRequest("Start date cannot be after end date");
                }

                var balanceHistory = await _balanceTrackingService.GetDailyBalanceHistory(userId, start, end);
                
                _logger.LogInformation("Retrieved balance history for user {UserId} from {StartDate} to {EndDate}", 
                    userId, start, end);

                return Ok(balanceHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving balance history");
                return StatusCode(500, "An error occurred while retrieving balance history");
            }
        }

        /// <summary>
        /// Get daily reserve history for the authenticated user
        /// </summary>
        /// <param name="startDate">Start date for the range (optional, defaults to 30 days ago)</param>
        /// <param name="endDate">End date for the range (optional, defaults to today)</param>
        /// <returns>List of daily reserve snapshots</returns>
        [HttpGet("reserve")]
        public async Task<ActionResult<IEnumerable<DailyReserveSnapshot>>> GetReserveHistory(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                if (start > end)
                {
                    return BadRequest("Start date cannot be after end date");
                }

                var reserveHistory = await _reserveTrackingService.GetDailyReserveHistory(userId, start, end);
                
                _logger.LogInformation("Retrieved reserve history for user {UserId} from {StartDate} to {EndDate}", 
                    userId, start, end);

                return Ok(reserveHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reserve history");
                return StatusCode(500, "An error occurred while retrieving reserve history");
            }
        }

        /// <summary>
        /// Get combined daily balance and reserve history for the authenticated user
        /// </summary>
        /// <param name="startDate">Start date for the range (optional, defaults to 30 days ago)</param>
        /// <param name="endDate">End date for the range (optional, defaults to today)</param>
        /// <returns>Combined balance and reserve history</returns>
        [HttpGet("combined")]
        public async Task<ActionResult<object>> GetCombinedHistory(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                if (start > end)
                {
                    return BadRequest("Start date cannot be after end date");
                }

                var balanceHistory = await _balanceTrackingService.GetDailyBalanceHistory(userId, start, end);
                var reserveHistory = await _reserveTrackingService.GetDailyReserveHistory(userId, start, end);

                var result = new
                {
                    BalanceHistory = balanceHistory,
                    ReserveHistory = reserveHistory,
                    DateRange = new { StartDate = start, EndDate = end }
                };

                _logger.LogInformation("Retrieved combined history for user {UserId} from {StartDate} to {EndDate}", 
                    userId, start, end);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving combined history");
                return StatusCode(500, "An error occurred while retrieving combined history");
            }
        }

        /// <summary>
        /// Get the latest balance snapshot for the authenticated user
        /// </summary>
        /// <returns>Latest balance snapshot</returns>
        [HttpGet("balance/latest")]
        public async Task<ActionResult<DailyBalanceSnapshot>> GetLatestBalance()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var latestBalance = await _balanceTrackingService.GetLatestBalanceSnapshot(userId);
                
                if (latestBalance == null)
                {
                    return NotFound("No balance snapshot found");
                }

                return Ok(latestBalance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest balance snapshot");
                return StatusCode(500, "An error occurred while retrieving latest balance snapshot");
            }
        }

        /// <summary>
        /// Get the latest reserve snapshot for the authenticated user
        /// </summary>
        /// <returns>Latest reserve snapshot</returns>
        [HttpGet("reserve/latest")]
        public async Task<ActionResult<DailyReserveSnapshot>> GetLatestReserve()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not authenticated");
                }

                var latestReserve = await _reserveTrackingService.GetLatestReserveSnapshot(userId);
                
                if (latestReserve == null)
                {
                    return NotFound("No reserve snapshot found");
                }

                return Ok(latestReserve);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest reserve snapshot");
                return StatusCode(500, "An error occurred while retrieving latest reserve snapshot");
            }
        }
    }
}
