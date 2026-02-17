using Microsoft.AspNetCore.Mvc;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Services.interfaces;

namespace API_SistemaLocacao.Controllers
{
    [ApiController]
    [Route("RentalReport")]
    public class RentalReportController : ControllerBase
    {
        private readonly IRentalReportService _reportService;
        private readonly IWebHostEnvironment _env;

        public RentalReportController(IRentalReportService reportService, IWebHostEnvironment env)
        {
            _reportService = reportService;
            _env = env;
        }

        [HttpGet("{RentalId:guid}/export/{format?}")]
        public async Task<IActionResult> Export(Guid RentalId, string? format)
        {
            try
            {
                var fmt = (format ?? "txt").ToLowerInvariant();
                byte[]? content;
                string extension;
                string contentType;

                if (fmt == "csv")
                {
                    content = await _reportService.ExportRentalReportCsvAsync(RentalId);
                    extension = "csv";
                    contentType = "text/csv";
                }
                else if (fmt == "txt")
                {
                    content = await _reportService.ExportRentalReportAsync(RentalId);
                    extension = "txt";
                    contentType = "text/plain";
                }
                else
                {
                    return BadRequest(new ProblemDetails
                    {
                        Status = 400,
                        Title = "Formato inválido",
                        Detail = "Use 'txt' ou 'csv'."
                    });
                }

                if (content == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Status = 404,
                        Title = "Relatório não encontrado",
                        Detail = $"Locação {RentalId} não encontrada."
                    });
                }

                // Usa caminho /bin/.../exports e cria pasta se não existir
                var exportsPath = Path.Combine(AppContext.BaseDirectory, "exports");

                Console.WriteLine($"PASTA: {exportsPath})" );

                Directory.CreateDirectory(exportsPath);

                var fileName = $"rental_report_{RentalId}.{extension}";
                var fullPath = Path.Combine(exportsPath, fileName);

                await System.IO.File.WriteAllBytesAsync(fullPath, content);

                return File(content, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Status = 500,
                    Title = "Erro interno",
                    Detail = ex.Message
                });
            }
        }

        [HttpGet("{RentalId:guid}")]
        public async Task<ActionResult<RentalReportResponseDTO>> GetReport(Guid RentalId)
        {
            var report = await _reportService.GetRentalReportAsync(RentalId);

            if (report == null)
                return NotFound($"Relatório {RentalId} não encontrado.");

            return Ok(report);
        }
    }
}
