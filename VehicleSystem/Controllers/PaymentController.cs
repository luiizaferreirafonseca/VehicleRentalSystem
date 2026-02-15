using Microsoft.AspNetCore.Mvc;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Services;
using VehicleRentalSystem.Services.interfaces;

namespace API_SistemaLocacao.Controllers;

[ApiController]
[Route("payments")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> Get(
        [FromQuery] Guid? rentalId,
        [FromQuery(Name = "paymentMethod")] string? method,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var payments = await _paymentService.GetAllPaymentsAsync(rentalId, method, startDate, endDate);

        return Ok(payments);
    }

    [HttpPatch("{rentalId:guid}/payments")]
    public async Task<IActionResult> RegisterPayment([FromRoute] Guid rentalId, [FromBody] PaymentCreateDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _paymentService.RegisterPaymentAsync(rentalId, dto);
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
}
