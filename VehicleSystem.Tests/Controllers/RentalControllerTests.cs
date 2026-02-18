using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_SistemaLocacao.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VehicleRentalSystem.Controllers;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Services.interfaces;

namespace VehicleSystem.Tests.Controllers
{
    [TestFixture]
    public class RentalControllerTests
    {
        private Mock<IRentalService> _service;
        private RentalController _controller;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<IRentalService>();
            _controller = new RentalController(_service.Object);
        }

        [Test]
        public async Task Create_ShouldReturn_400BadRequest_WhenModelStateIsInvalid()
        {
            var dto = new RentalCreateDTO();
            _controller.ModelState.AddModelError("UserId", "Required");

            var result = await _controller.Create(dto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Create_ShouldReturn_201CreatedAtAction_WhenRentalCreatedSuccessfully()
        {
            var createdId = Guid.NewGuid();
            var dto = new RentalCreateDTO
            {
                UserId = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                StartDate = new DateTime(2026, 02, 02),
                ExpectedEndDate = new DateTime(2026, 02, 05)
            };

            var created = new RentalResponseDTO
            {
                Id = createdId,
                UserId = dto.UserId,
                VehicleId = dto.VehicleId
            };

            _service.Setup(s => s.CreateRentalAsync(dto))
                        .ReturnsAsync(created);

            var result = await _controller.Create(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<CreatedAtActionResult>());

                var createdAt = result as CreatedAtActionResult;
                Assert.That(createdAt, Is.Not.Null);

                Assert.That(createdAt!.ActionName, Is.EqualTo("GetById"));
                Assert.That(createdAt.RouteValues, Is.Not.Null);
                Assert.That(createdAt.RouteValues!["id"], Is.EqualTo(createdId));

                var body = createdAt.Value as RentalResponseDTO;
                Assert.That(body, Is.Not.Null);
                Assert.That(body!.Id, Is.EqualTo(createdId));
            });

            _service.Verify(s => s.CreateRentalAsync(dto), Times.Once);
        }

        [Test]
        public async Task Create_ShouldReturn_404NotFound_WhenKeyNotFoundExceptionOccurs()
        {
            var dto = new RentalCreateDTO
            {
                UserId = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                StartDate = new DateTime(2026, 02, 02),
                ExpectedEndDate = new DateTime(2026, 02, 05)
            };

            _service.Setup(s => s.CreateRentalAsync(dto))
                        .ThrowsAsync(new KeyNotFoundException("Usuário não encontrado"));

            var result = await _controller.Create(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<NotFoundObjectResult>());

                var notFound = result as NotFoundObjectResult;
                Assert.That(notFound, Is.Not.Null);

                var problem = notFound!.Value as ProblemDetails;
                Assert.That(problem, Is.Not.Null);
                Assert.That(problem!.Status, Is.EqualTo(StatusCodes.Status404NotFound));
                Assert.That(problem.Title, Is.EqualTo("No encontrado"));
                Assert.That(problem.Detail, Is.EqualTo("Usuário não encontrado"));
            });
        }

        [Test]
        public async Task Create_ShouldReturn_500_WhenUnexpectedExceptionOccurs()
        {
            var dto = new RentalCreateDTO
            {
                UserId = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                StartDate = new DateTime(2026, 02, 02),
                ExpectedEndDate = new DateTime(2026, 02, 05)
            };

            _service.Setup(s => s.CreateRentalAsync(dto))
                        .ThrowsAsync(new Exception("Falha inesperada"));

            var result = await _controller.Create(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<ObjectResult>());

                var obj = result as ObjectResult;
                Assert.That(obj, Is.Not.Null);
                Assert.That(obj!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));

                var problem = obj.Value as ProblemDetails;
                Assert.That(problem, Is.Not.Null);
                Assert.That(problem!.Status, Is.EqualTo(StatusCodes.Status500InternalServerError));
                Assert.That(problem.Title, Is.EqualTo("Erro interno do servidor"));
                Assert.That(problem.Detail, Is.EqualTo("Falha inesperada"));
            });
        }

        [Test]
        public async Task Return_ShouldReturn_404NotFound_WhenRentalDoesNotExist()
        {
            var rentalId = Guid.NewGuid();

            _service.Setup(s => s.ReturnRentalAsync(rentalId))
                        .ThrowsAsync(new KeyNotFoundException("Locação não encontrada"));

            var result = await _controller.Return(rentalId);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<NotFoundObjectResult>());

                var notFound = result as NotFoundObjectResult;
                Assert.That(notFound, Is.Not.Null);

                var problem = notFound!.Value as ProblemDetails;
                Assert.That(problem, Is.Not.Null);
                Assert.That(problem!.Status, Is.EqualTo(StatusCodes.Status404NotFound));
                Assert.That(problem.Title, Is.EqualTo("Erro! Locação não encontrada"));
                Assert.That(problem.Detail, Is.EqualTo("Locação não encontrada"));
            });

            _service.Verify(s => s.ReturnRentalAsync(rentalId), Times.Once);
        }
    }
}
