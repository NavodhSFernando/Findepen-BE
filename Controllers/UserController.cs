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
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Correct claim type

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "User ID not found in claims" });

            return Ok(new { UserId = userId, Message = "Protected resource accessed!" });
        }


    }
}