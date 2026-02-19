using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class PaymentControllerTests
    {
        private Mock<IPaymentService> _paymentServiceMock;
        private PaymentController _controller;

        [SetUp]
        public void SetUp()
        {
            _paymentServiceMock = new Mock<IPaymentService>();
            _controller = new PaymentController(_paymentServiceMock.Object);
        }

        // --- GET /payments ---

        // Teste (NUnit)
        [Test]
        [Category("Unit")]
        [Property("Priority", "Medium")]
        public async Task Get_SemFiltros_DeveRetornarOkComListaDePagamentos()
        {
            // Arrange
            var payments = new List<PaymentResponseDto>
            {
                new PaymentResponseDto { Id = Guid.NewGuid(), Amount = 100m },
                new PaymentResponseDto { Id = Guid.NewGuid(), Amount = 200m }
            };

            _paymentServiceMock
                .Setup(s => s.GetAllPaymentsAsync(null, null, null, null))
                .ReturnsAsync(payments);

            // Act
            var result = await _controller.Get(null, null, null, null);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
                var okResult = result.Result as OkObjectResult;
                Assert.That(okResult!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
                Assert.That(okResult.Value, Is.EqualTo(payments));
            });

            _paymentServiceMock.Verify(
                s => s.GetAllPaymentsAsync(null, null, null, null),
                Times.Once);
        }

        // Teste (NUnit)
        [Test]
        [Category("Unit")]
        [Property("Priority", "Medium")]
        public async Task Get_ComFiltros_DevePassarParametrosCorretamenteParaServico()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            const string method = "PIX";
            var startDate = new DateTime(2024, 1, 1);
            var endDate = new DateTime(2024, 1, 31);

            var payments = new List<PaymentResponseDto>();

            _paymentServiceMock
                .Setup(s => s.GetAllPaymentsAsync(rentalId, method, startDate, endDate))
                .ReturnsAsync(payments);

            // Act
            var result = await _controller.Get(rentalId, method, startDate, endDate);

            // Assert
            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
            _paymentServiceMock.Verify(
                s => s.GetAllPaymentsAsync(rentalId, method, startDate, endDate),
                Times.Once);
        }

        // --- PATCH /payments/{rentalId} ---

        // Teste (NUnit)
        [Test]
        [Category("Unit")]
        [Property("Priority", "High")]
        public async Task RegisterPayment_DadosValidos_DeveRetornarOkComResultado()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var expectedDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var dto = new PaymentCreateDTO
            {
                Amount = 150m,
                PaymentMethod = VehicleRentalSystem.Enums.EnumPaymentMethod.CREDIT_CARD
            };

            var response = new PaymentResponseDto
            {
                Id = Guid.NewGuid(),
                RentalId = rentalId,
                Amount = dto.Amount,
                PaymentMethod = dto.PaymentMethod!.Value.ToString(),
                PaymentDate = expectedDate
            };

            _paymentServiceMock
                .Setup(s => s.RegisterPaymentAsync(rentalId, dto))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.RegisterPayment(rentalId, dto);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<OkObjectResult>());
                var okResult = result as OkObjectResult;
                Assert.That(okResult!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
                var resultDto = okResult.Value as PaymentResponseDto;
                Assert.That(resultDto, Is.Not.Null);
                Assert.That(resultDto!.Id, Is.EqualTo(response.Id));
                Assert.That(resultDto.RentalId, Is.EqualTo(response.RentalId));
                Assert.That(resultDto.Amount, Is.EqualTo(response.Amount));
                Assert.That(resultDto.PaymentMethod, Is.EqualTo(response.PaymentMethod));
                Assert.That(resultDto.PaymentDate, Is.EqualTo(expectedDate));
            });

            _paymentServiceMock.Verify(
                s => s.RegisterPaymentAsync(rentalId, dto),
                Times.Once);
        }

        // Teste (NUnit)
        [Test]
        [Category("Unit")]
        [Property("Priority", "Medium")]
        public async Task RegisterPayment_ModelStateInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var dto = new PaymentCreateDTO();
            _controller.ModelState.AddModelError("Amount", "Required");

            // Act
            var result = await _controller.RegisterPayment(rentalId, dto);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            _paymentServiceMock.Verify(
                s => s.RegisterPaymentAsync(It.IsAny<Guid>(), It.IsAny<PaymentCreateDTO>()),
                Times.Never);
        }

        // Teste (NUnit)
        [Test]
        [Category("Unit")]
        [Property("Priority", "High")]
        public async Task RegisterPayment_AluguelNaoEncontrado_DeveRetornarNotFound()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var dto = new PaymentCreateDTO { Amount = 100m, PaymentMethod = VehicleRentalSystem.Enums.EnumPaymentMethod.PIX };

            _paymentServiceMock
                .Setup(s => s.RegisterPaymentAsync(rentalId, dto))
                .ThrowsAsync(new KeyNotFoundException("Rental not found"));

            // Act
            var result = await _controller.RegisterPayment(rentalId, dto);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
                var notFound = result as NotFoundObjectResult;
                var problem = notFound!.Value as ProblemDetails;
                Assert.That(problem, Is.Not.Null);
                Assert.That(problem!.Status, Is.EqualTo(StatusCodes.Status404NotFound));
                Assert.That(problem.Title, Is.EqualTo(Messages.RentalNotFound));
            });

            _paymentServiceMock.Verify(
                s => s.RegisterPaymentAsync(rentalId, dto),
                Times.Once);
        }

        // Teste (NUnit)
        [Test]
        [Category("Unit")]
        [Property("Priority", "High")]
        public async Task RegisterPayment_OperacaoInvalida_DeveRetornarBadRequest()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var dto = new PaymentCreateDTO { Amount = 100m, PaymentMethod = VehicleRentalSystem.Enums.EnumPaymentMethod.PIX };

            _paymentServiceMock
                .Setup(s => s.RegisterPaymentAsync(rentalId, dto))
                .ThrowsAsync(new InvalidOperationException("Invalid operation"));

            // Act
            var result = await _controller.RegisterPayment(rentalId, dto);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
                var badRequest = result as BadRequestObjectResult;
                var problem = badRequest!.Value as ProblemDetails;
                Assert.That(problem, Is.Not.Null);
                Assert.That(problem!.Status, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(problem.Title, Is.EqualTo(Messages.InvalidOperation));
            });

            _paymentServiceMock.Verify(
                s => s.RegisterPaymentAsync(rentalId, dto),
                Times.Once);
        }

        // Teste (NUnit)
        [Test]
        [Category("Unit")]
        [Property("Priority", "Low")]
        public async Task RegisterPayment_ExcecaoNaoTratada_DeveRetornarErro500()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var dto = new PaymentCreateDTO { Amount = 100m, PaymentMethod = VehicleRentalSystem.Enums.EnumPaymentMethod.PIX };

            _paymentServiceMock
                .Setup(s => s.RegisterPaymentAsync(rentalId, dto))
                .ThrowsAsync(new Exception("Unexpected"));

            // Act
            var result = await _controller.RegisterPayment(rentalId, dto);

            // Assert
            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));

            var problem = objectResult.Value as ProblemDetails;
            Assert.That(problem, Is.Not.Null);
            Assert.That(problem!.Title, Is.EqualTo(Messages.ServerError));

            _paymentServiceMock.Verify(
                s => s.RegisterPaymentAsync(rentalId, dto),
                Times.Once);
        }
    }
}
