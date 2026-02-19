using Microsoft.AspNetCore.Mvc;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Services.interfaces;

namespace VehicleRentalSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        /// <summary>
        /// Submits a rating for a rental and evaluates the provided feedback.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PostRating([FromBody] RatingCreateDTO dto)
        {
            try
            {
                var success = await _ratingService.EvaluateRentalAsync(dto);
                return Ok(new { message = "Review submitted successfully! Thank you for collaborating with us." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
