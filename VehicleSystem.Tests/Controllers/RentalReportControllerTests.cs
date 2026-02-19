using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VehicleRentalSystem.Controllers;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Resources;
using VehicleRentalSystem.Services.interfaces;

namespace VehicleSystem.Tests.Controllers
{
    [TestFixture]
    [Category("Controllers")]
    public class RentalReportControllerTests
    {
        private Mock<IRentalReportService> _reportServiceMock;
        private Mock<IWebHostEnvironment> _envMock;
        private Mock<ILogger<RentalReportController>> _loggerMock;
        private RentalReportController _controller;
        private string _tempPath;

        [SetUp]
        public void SetUp()
        {
            _reportServiceMock = new Mock<IRentalReportService>();
            _envMock = new Mock<IWebHostEnvironment>();
            _loggerMock = new Mock<ILogger<RentalReportController>>();

            _tempPath = Path.Combine(Path.GetTempPath(), "rental_report_tests");
            _envMock.Setup(e => e.ContentRootPath).Returns(_tempPath);

            _controller = new RentalReportController(
                _reportServiceMock.Object,
                _envMock.Object,
                _loggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            var exportsPath = Path.Combine(_tempPath, "exports");
            if (Directory.Exists(exportsPath))
                Directory.Delete(exportsPath, recursive: true);
        }

        #region Export

        [Test]
        [Category("Unit")]
        public async Task Export_FormatTxt_ReturnsFileContentResultWithCorrectContentType()
        {
            var rentalId = Guid.NewGuid();
            var content = System.Text.Encoding.UTF8.GetBytes("report content");

            _reportServiceMock
                .Setup(s => s.ExportRentalReportAsync(rentalId))
                .ReturnsAsync(content);

            var result = await _controller.Export(rentalId, "txt");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<FileContentResult>());
                var file = (FileContentResult)result;
                Assert.That(file.ContentType, Is.EqualTo("text/plain"));
                Assert.That(file.FileDownloadName, Is.EqualTo($"rental_report_{rentalId}.txt"));
                Assert.That(file.FileContents, Is.EqualTo(content));
            });

            _reportServiceMock.Verify(s => s.ExportRentalReportAsync(rentalId), Times.Once);
            _reportServiceMock.Verify(s => s.ExportRentalReportCsvAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        [Category("Unit")]
        public async Task Export_FormatNull_DefaultsToTxt()
        {
            var rentalId = Guid.NewGuid();
            var content = System.Text.Encoding.UTF8.GetBytes("report content");

            _reportServiceMock
                .Setup(s => s.ExportRentalReportAsync(rentalId))
                .ReturnsAsync(content);

            var result = await _controller.Export(rentalId, null);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<FileContentResult>());
                var file = (FileContentResult)result;
                Assert.That(file.ContentType, Is.EqualTo("text/plain"));
                Assert.That(file.FileDownloadName, Is.EqualTo($"rental_report_{rentalId}.txt"));
            });

            _reportServiceMock.Verify(s => s.ExportRentalReportAsync(rentalId), Times.Once);
        }

        [Test]
        [Category("Unit")]
        public async Task Export_FormatCsv_ReturnsFileContentResultWithCsvContentType()
        {
            var rentalId = Guid.NewGuid();
            var content = System.Text.Encoding.UTF8.GetBytes("id,amount\n1,100");

            _reportServiceMock
                .Setup(s => s.ExportRentalReportCsvAsync(rentalId))
                .ReturnsAsync(content);

            var result = await _controller.Export(rentalId, "csv");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<FileContentResult>());
                var file = (FileContentResult)result;
                Assert.That(file.ContentType, Is.EqualTo("text/csv"));
                Assert.That(file.FileDownloadName, Is.EqualTo($"rental_report_{rentalId}.csv"));
                Assert.That(file.FileContents, Is.EqualTo(content));
            });

            _reportServiceMock.Verify(s => s.ExportRentalReportCsvAsync(rentalId), Times.Once);
            _reportServiceMock.Verify(s => s.ExportRentalReportAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        [Category("BusinessRule")]
        public async Task Export_InvalidFormat_ReturnsBadRequest()
        {
            var rentalId = Guid.NewGuid();

            var result = await _controller.Export(rentalId, "pdf");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
                var badRequest = (BadRequestObjectResult)result;
                Assert.That(badRequest.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
                var problem = (ProblemDetails)badRequest.Value!;
                Assert.That(problem.Title, Is.EqualTo(Messages.InvalidFormat));
            });

            _reportServiceMock.Verify(s => s.ExportRentalReportAsync(It.IsAny<Guid>()), Times.Never);
            _reportServiceMock.Verify(s => s.ExportRentalReportCsvAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        [Category("Unit")]
        public async Task Export_ServiceReturnsNull_ReturnsNotFound()
        {
            var rentalId = Guid.NewGuid();

            _reportServiceMock
                .Setup(s => s.ExportRentalReportAsync(rentalId))
                .ReturnsAsync((byte[]?)null);

            var result = await _controller.Export(rentalId, "txt");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
                var notFound = (NotFoundObjectResult)result;
                Assert.That(notFound.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
                var problem = (ProblemDetails)notFound.Value!;
                Assert.That(problem.Title, Is.EqualTo(Messages.ReportNotFound));
                Assert.That(problem.Detail, Does.Contain(rentalId.ToString()));
            });
        }

        [Test]
        [Category("Unit")]
        public async Task Export_ServiceThrowsException_Returns500()
        {
            var rentalId = Guid.NewGuid();

            _reportServiceMock
                .Setup(s => s.ExportRentalReportAsync(rentalId))
                .ThrowsAsync(new Exception("unexpected error"));

            var result = await _controller.Export(rentalId, "txt");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<ObjectResult>());
                var objectResult = (ObjectResult)result;
                Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
                var problem = (ProblemDetails)objectResult.Value!;
                Assert.That(problem.Title, Is.EqualTo(Messages.ServerError));
                Assert.That(problem.Detail, Is.EqualTo(Messages.UnexpectedServerErrorDetail));
            });
        }

        [Test]
        [Category("Unit")]
        public async Task Export_ValidContent_WritesFileToDisk()
        {
            var rentalId = Guid.NewGuid();
            var content = System.Text.Encoding.UTF8.GetBytes("file content");

            _reportServiceMock
                .Setup(s => s.ExportRentalReportAsync(rentalId))
                .ReturnsAsync(content);

            await _controller.Export(rentalId, "txt");

            var expectedFile = Path.Combine(_tempPath, "exports", $"rental_report_{rentalId}.txt");
            Assert.That(File.Exists(expectedFile), Is.True);
            Assert.That(await File.ReadAllBytesAsync(expectedFile), Is.EqualTo(content));
        }

        #endregion

        #region GetReport

        [Test]
        [Category("Unit")]
        public async Task GetReport_ReportExists_ReturnsOkWithReport()
        {
            var rentalId = Guid.NewGuid();
            var report = new RentalReportResponseDTO
            {
                RentalId = rentalId,
                StartDate = DateTime.UtcNow.AddDays(-3),
                EndDate = DateTime.UtcNow,
                TotalAmount = 300m,
                Status = "completed"
            };

            _reportServiceMock
                .Setup(s => s.GetRentalReportAsync(rentalId))
                .ReturnsAsync(report);

            var result = await _controller.GetReport(rentalId);

            Assert.Multiple(() =>
            {
                Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
                var ok = (OkObjectResult)result.Result!;
                Assert.That(ok.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
                Assert.That(ok.Value, Is.EqualTo(report));
            });

            _reportServiceMock.Verify(s => s.GetRentalReportAsync(rentalId), Times.Once);
        }

        [Test]
        [Category("Unit")]
        public async Task GetReport_ReportNotFound_ReturnsNotFound()
        {
            var rentalId = Guid.NewGuid();

            _reportServiceMock
                .Setup(s => s.GetRentalReportAsync(rentalId))
                .ReturnsAsync((RentalReportResponseDTO?)null);

            var result = await _controller.GetReport(rentalId);

            Assert.Multiple(() =>
            {
                Assert.That(result.Result, Is.TypeOf<NotFoundObjectResult>());
                var notFound = (NotFoundObjectResult)result.Result!;
                Assert.That(notFound.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
                var problem = (ProblemDetails)notFound.Value!;
                Assert.That(problem.Title, Is.EqualTo(Messages.ReportNotFound));
                Assert.That(problem.Detail, Does.Contain(rentalId.ToString()));
            });

            _reportServiceMock.Verify(s => s.GetRentalReportAsync(rentalId), Times.Once);
        }

        #endregion
    }
}
