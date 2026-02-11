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
                Title = "No encontrado",
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

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            var result = await _service.CancelRentalAsync(id);
            
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Erro!! Locação não encontrada",
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Erro!! Operação Inválida",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro de servidor",
                Detail = ex.Message
            });
        }
    }

    [HttpPatch("{id:guid}/update-dates")]
    public async Task<IActionResult> UpdateDates(Guid id, [FromBody] UpdateRentalDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _service.UpdateRentalDatesAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Locação não encontrada",
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Operação Inválida",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro de servidor",
                Detail = ex.Message
            });
        }
    }
}