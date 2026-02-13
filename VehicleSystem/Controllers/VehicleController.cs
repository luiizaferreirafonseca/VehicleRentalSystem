using Microsoft.AspNetCore.Mvc;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Services.interfaces;

namespace API_SistemaLocacao.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _service;

        public VehicleController(IVehicleService service)
        {
            _service = service;
        }

        [HttpPost(Name = "CreateVehicle")]
        public async Task<IActionResult> Create([FromBody] VehicleCreateDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var created = await _service.CreateVehicleAsync(request);

                return StatusCode(StatusCodes.Status201Created, created);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflito",
                    Detail = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Erro interno do servidor",
                    Detail = ex.Message
                });
            }
        }
    }
}
