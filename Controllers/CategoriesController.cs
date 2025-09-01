using FinDepen_Backend.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinDepen_Backend.Controllers
{
    [Route("api/categories")]
    [ApiController]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ILogger<CategoriesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetCategories()
        {
            try
            {
                _logger.LogInformation("Retrieving valid expense categories");
                return Ok(Categories.ValidCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, "An error occurred while retrieving categories");
            }
        }
    }
} 