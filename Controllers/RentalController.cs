using Microsoft.AspNetCore.Mvc;
using VehicleRentalSystem;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Services;

namespace API_SistemaLocacao.Controllers;

[ApiController]
[Route("[controller]")]
public class RentalController : ControllerBase
{
    private IRentalService _service;

    public RentalController(IRentalService service)
    {
        _service = service;
    }

    [HttpGet(Name = "GetAllRentals")]
    public List<RentalResponseDTO> Get()
    {
        return _service.GetRentals();
    }

    [HttpGet("{id:guid}")]
    public RentalResponseDTO GetById(Guid id)
    {
        return _service.GetRentalById(id);
    }

    [HttpPost(Name = "CreateRental")]
    public async Task<IActionResult> Create([FromBody] RentalCreateDTO request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdRental = await _service.CreateRentalAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = createdRental.Id }, createdRental);
        }
        catch (InvalidOperationException ex)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflito",
                Detail = ex.Message
            };
            return Conflict(problemDetails);
        }
        catch (KeyNotFoundException ex)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Não encontrado",
                Detail = ex.Message
            };
            return NotFound(problemDetails);
        }
        catch (Exception ex)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro interno do servidor",
                Detail = ex.Message
            };
            return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
        }
    }

}
