using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using VehicleRentalSystem;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Services.interfaces;

namespace VehicleRentalSystem.Controllers;

[ApiController]
[Route("[controller]")]
public class RentalController : ControllerBase
{
    private IRentalService _service;

    public RentalController(IRentalService service)
    {
        _service = service;
    }

    /// <summary>
    /// Returns the list of rentals.
    /// </summary>
    [HttpGet(Name = "GetAllRentals")]
    public async Task<IActionResult> Get()
    {
        try
        {
            var rentals = await _service.GetRentalsAsync();
            return Ok(rentals);
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

    /// <summary>
    /// Returns a rental by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var rental = await _service.GetRentalByIdAsync(id);
            return Ok(rental);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Operação inválida",
                Detail = ex.Message
            });
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

    /// <summary>
    /// Creates a new rental.
    /// </summary>
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

    /// <summary>
    /// Cancels a rental.
    /// </summary>
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

    /// <summary>
    /// Updates the rental dates.
    /// </summary>
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

    /// <summary>
    /// Returns a rental.
    /// </summary>
    [HttpPatch("{id:guid}/return")]
    public async Task<IActionResult> Return(Guid id)
    {
        try
        {
            var result = await _service.ReturnRentalAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Erro! Locação não encontrada",
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Erro! Operação Inválida",
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

    /// <summary>
    /// Searches rentals by user (and optional status) with pagination.
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] Guid userId, [FromQuery] string? status, [FromQuery] int page = 1)
    {
        try
        {
            var result = await _service.SearchRentalsByUserAsync(userId, status, page);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid operation",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal server error",
                Detail = ex.Message
            });
        }
    }
}