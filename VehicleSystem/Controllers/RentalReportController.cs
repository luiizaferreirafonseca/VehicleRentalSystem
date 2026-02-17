using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Services.interfaces;
using VehicleRentalSystem.Resources;
using Microsoft.AspNetCore.Http;

namespace API_SistemaLocacao.Controllers;

[ApiController]
[Route("report")]
public class RentalReportController : ControllerBase
{
    private readonly IRentalReportService _reportService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<RentalReportController> _logger;

    public RentalReportController(IRentalReportService reportService, IWebHostEnvironment env, ILogger<RentalReportController> logger)
    {
        _reportService = reportService;
        _env = env;
        _logger = logger;
    }

    [HttpGet("{rentalId:guid}/export/{format?}")]
    public async Task<IActionResult> Export(Guid rentalId, string? format)
    {
        try
        {
            var fmt = (format ?? "txt").ToLowerInvariant();
            byte[]? content;
            string extension;
            string contentType;

            if (fmt == "csv")
            {
                content = await _reportService.ExportRentalReportCsvAsync(rentalId);
                extension = "csv";
                contentType = "text/csv";
            }
            else if (fmt == "txt")
            {
                content = await _reportService.ExportRentalReportAsync(rentalId);
                extension = "txt";
                contentType = "text/plain";
            }
            else
            {
                return BadRequest(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = Messages.InvalidFormat,
                    Detail = Messages.InvalidFormat
                });
            }

            if (content == null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = Messages.ReportNotFound,
                    Detail = string.Format(Messages.ReportNotFoundDetailFormat, rentalId)
                });
            }

            // Usa ContentRootPath e cria pasta se não existir
            var exportsPath = Path.Combine(_env.ContentRootPath, "exports");

            _logger.LogInformation("Export - exportsPath: {exportsPath}", exportsPath);

            Directory.CreateDirectory(exportsPath);

            var fileName = $"rental_report_{rentalId}.{extension}";
            var fullPath = Path.Combine(exportsPath, fileName);

            await System.IO.File.WriteAllBytesAsync(fullPath, content);

            return File(content, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export - unexpected error for RentalId={rentalId}", rentalId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = Messages.ServerError,
                Detail = Messages.UnexpectedServerErrorDetail
            });
        }
    }

    [HttpGet("{rentalId:guid}")]
    public async Task<ActionResult<RentalReportResponseDTO>> GetReport(Guid rentalId)
    {
        var report = await _reportService.GetRentalReportAsync(rentalId);

        if (report == null)
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = Messages.ReportNotFound,
                Detail = string.Format(Messages.ReportNotFoundDetailFormat, rentalId)
            });

        return Ok(report);
    }
}