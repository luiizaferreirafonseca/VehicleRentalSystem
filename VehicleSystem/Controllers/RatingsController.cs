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

        [HttpPost]
        public async Task<IActionResult> PostRating([FromBody] RatingCreateDTO dto)
        {
            try
            {
                var success = await _ratingService.EvaluateRentalAsync(dto);
                return Ok(new { message = "Avaliação enviada com sucesso! Obrigado por colaborar conosco." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}