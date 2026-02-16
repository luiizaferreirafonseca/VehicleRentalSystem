using Microsoft.AspNetCore.Mvc;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Services;
using VehicleRentalSystem.Services.interfaces;

namespace API_SistemaLocacao.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RentalReportController : ControllerBase
    {
        private readonly IRentalReportService _reportService;

        public RentalReportController(IRentalReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RentalReportResponseDTO>> GetReport(Guid id)
        {
            try
            {
                var report = await _reportService.GetRentalReportAsync(id);

                if (report == null)
                {

                    return NotFound(new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "Relatório não encontrado",
                        Detail = $"Não foi possível localizar uma locação com o ID: {id}"
                    });
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Erro interno ao gerar relatório",
                    Detail = ex.Message
                });
            }
        }
    }
}