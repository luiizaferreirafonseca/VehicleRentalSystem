using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Services.interfaces;
using VehicleRentalSystem.Resources;

namespace VehicleRentalSystem.Controllers;

[ApiController]
[Route("accessories")]
public class AccessoryController : ControllerBase
{
    private readonly IAccessoryService _accessoryService;
    private readonly ILogger<AccessoryController> _logger;

    public AccessoryController(IAccessoryService accessoryService, ILogger<AccessoryController> logger)
    {
        _accessoryService = accessoryService;
        _logger = logger;
    }

    /// <summary>
    /// Recupera a lista de todos os acessórios disponíveis no sistema.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccessoryResponseDto>>> Get()
    {
        var accessories = await _accessoryService.GetAccessoriesAsync();
        return Ok(accessories);
    }

    /// <summary>
    /// Busca um acessório específico através de seu identificador único (GUID).
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AccessoryResponseDto>> GetById(Guid id)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var accessory = await _accessoryService.GetAccessoryByIdAsync(id);
            return Ok(accessory);
        }
        catch (KeyNotFoundException ex)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = Messages.NotFound,
                Detail = ex.Message
            };
            return NotFound(problemDetails);
        }
        catch (InvalidOperationException ex)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = Messages.Conflict,
                Detail = ex.Message
            };
            return Conflict(problemDetails);
        }
        catch (Exception)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = Messages.ServerInternalError,
                Detail = Messages.UnexpectedServerErrorDetail
            };
            return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
        }
    }

    /// <summary>
    /// Cria um novo registro de acessório. Valida se o nome já existe (Conflito).
    /// </summary>
    [HttpPost("add")]
    public async Task<ActionResult<AccessoryResponseDto>> Create([FromBody] AccessoryCreateDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var newAccessory = await _accessoryService.CreateAccessoryAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = newAccessory.Id }, newAccessory);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = Messages.Conflict,
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = Messages.ServerInternalError,
                Detail = ex.Message
            };
            return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
        }
    }

    /// <summary>
    /// Vincula um acessório a um contrato de aluguel existente.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> AddAccessoryToRental([FromBody] RentalAccessoryRequestDto request)
    {
        try
        {
            _logger.LogInformation("AddAccessoryToRental called with RentalId={rentalId} AccessoryId={accessoryid}", request?.RentalId, request?.AccessoryId);

            if (request == null)
            {
                _logger.LogWarning("AddAccessoryToRental called with empty body");
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = Messages.RequestInvalid,
                    Detail = Messages.RequestBodyEmptyDetail
                });
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.RentalId == Guid.Empty || request.AccessoryId == Guid.Empty)
            {
                _logger.LogWarning("AddAccessoryToRental invalid ids RentalId={rentalId} AccessoryId={accessoryid}", request.RentalId, request.AccessoryId);
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = Messages.IdsInvalid,
                    Detail = Messages.IdsInvalid
                });
            }

            await _accessoryService.AddAccessoryToRentalAsync(request.RentalId, request.AccessoryId);
            _logger.LogInformation("AddAccessoryToRental succeeded for RentalId={rentalId} AccessoryId={accessoryId}", request.RentalId, request.AccessoryId);

            return Ok(new { Message = Messages.AccessoryLinkedSuccess });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "AddAccessoryToRental - NotFound for RentalId={rentalId} AccessoryId={accessoryId}", request.RentalId, request.AccessoryId);
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = Messages.NotFound,
                Detail = ex.Message
            };
            return NotFound(problemDetails);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "AddAccessoryToRental - Conflict for RentalId={rentalId} AccessoryId={accessoryId}", request.RentalId, request.AccessoryId);
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = Messages.Conflict,
                Detail = ex.Message
            };
            return Conflict(problemDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddAccessoryToRental - Unexpected error for RentalId={rentalId} AccessoryId={accessoryId}", request.RentalId, request.AccessoryId);
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = Messages.ServerInternalError,
                Detail = Messages.UnexpectedServerErrorDetail
            };
            return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
        }
    }

    /// <summary>
    /// Lista todos os acessórios vinculados a um aluguel específico.
    /// </summary>
    [HttpGet("~/rental/{id:guid}/accessories")]
    public async Task<ActionResult<IEnumerable<AccessoryResponseDto>>> GetAccessoriesByRental(Guid id)
    {
        try
        {
            var accessories = await _accessoryService.GetAccessoriesByRentalIdAsync(id);
            return Ok(accessories);
        }
        catch (KeyNotFoundException ex)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = Messages.NotFound,
                Detail = ex.Message
            };
            return NotFound(problemDetails);
        }
        catch (InvalidOperationException ex)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = Messages.Conflict,
                Detail = ex.Message
            };
            return Conflict(problemDetails);
        }
        catch (Exception)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = Messages.ServerInternalError,
                Detail = Messages.UnexpectedServerErrorDetail
            };
            return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
        }
    }

    /// <summary>
    /// Remove o vínculo entre um acessório e um aluguel.
    /// </summary>
    [HttpDelete("~/rental/{rentalId:guid}/accessories/{accessoryId:guid}")]
    public async Task<ActionResult> RemoveAccessoryFromRental(Guid rentalId, Guid accessoryId)
    {
        try
        {
            await _accessoryService.RemoveAccessoryFromRentalAsync(rentalId, accessoryId);
            _logger.LogInformation("RemoveAccessoryFromRental succeeded for RentalId={rentalId} AccessoryId={accessoryId}", rentalId, accessoryId);
            return Ok(new { Message = Messages.AccessoryUnlinkedSuccess });
        }
        catch (KeyNotFoundException ex)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = Messages.NotFound,
                Detail = ex.Message
            };
            return NotFound(problemDetails);
        }
        catch (InvalidOperationException ex)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = Messages.Conflict,
                Detail = ex.Message
            };
            return Conflict(problemDetails);
        }
        catch (Exception)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = Messages.ServerInternalError,
                Detail = Messages.UnexpectedServerErrorDetail
            };
            return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
        }
    }
}