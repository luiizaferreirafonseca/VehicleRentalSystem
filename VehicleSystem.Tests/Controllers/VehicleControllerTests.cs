using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Services.interfaces;
using API_SistemaLocacao.Controllers; 
using System;

namespace VehicleSystem.Tests.Controllers
{
    [TestFixture]
    public class VehicleControllerTests
    {
        private Mock<IVehicleService> _service;
        private VehicleController _controller;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<IVehicleService>();
            _controller = new VehicleController(_service.Object);
        }

        [Test]
        public async Task CreateVehicle_ServiceRetornaSucesso_DeveRetornar201ComBody()
        {
            var dto = new VehicleCreateDTO
            {
                Brand = "Chevrolet",
                Model = "Onix",
                Year = 2022,
                DailyRate = 150m,
                LicensePlate = "ABC1234"
            };

            var created = new VehicleResponseDTO
            {
                Id = Guid.NewGuid(),
                Brand = dto.Brand,
                Model = dto.Model,
                Year = dto.Year,
                DailyRate = dto.DailyRate,
                LicensePlate = dto.LicensePlate,
                Status = "available"
            };

            _service.Setup(s => s.CreateVehicleAsync(dto))
                    .ReturnsAsync(created);

            var result = await _controller.Create(dto);

            Assert.That(result, Is.TypeOf<ObjectResult>());

            var obj = result as ObjectResult;
            Assert.That(obj.StatusCode, Is.EqualTo(StatusCodes.Status201Created));

            var body = obj.Value as VehicleResponseDTO;
            Assert.That(body.Id, Is.EqualTo(created.Id));
            Assert.That(body.Status, Is.EqualTo("available"));
        }

        [Test]
        public async Task CreateVehicle_ServiceLancaInvalidOperation_DeveRetornarConflictComProblemDetails()
        {
            var dto = new VehicleCreateDTO
            {
                Brand = "Chevrolet",
                Model = "Onix",
                Year = 2022,
                DailyRate = 150m,
                LicensePlate = "ABC1234"
            };

            _service.Setup(s => s.CreateVehicleAsync(dto))
                    .ThrowsAsync(new InvalidOperationException("conflito"));

            var result = await _controller.Create(dto);

            Assert.That(result, Is.TypeOf<ConflictObjectResult>());

            var conflict = result as ConflictObjectResult;
            var problem = conflict.Value as ProblemDetails;

            Assert.That(problem.Status, Is.EqualTo(StatusCodes.Status409Conflict));
            Assert.That(problem.Title, Is.EqualTo("Conflito"));
            Assert.That(problem.Detail, Is.EqualTo("conflito"));
        }

        [Test]
        public async Task Delete_ShouldReturn_204NoContent_WhenRemovedSuccessfully()
        {
            var id = Guid.NewGuid();
            _service.Setup(s => s.RemoveVehicleAsync(id)).Returns(Task.CompletedTask);

            var result = await _controller.Delete(id);

            Assert.That(result, Is.TypeOf<NoContentResult>());
            _service.Verify(s => s.RemoveVehicleAsync(id), Times.Once);
        }

        [Test]
        public async Task Delete_ShouldReturn_404NotFound_WhenKeyNotFoundExceptionOccurs()
        {
            var id = Guid.NewGuid();
            _service.Setup(s => s.RemoveVehicleAsync(id))
                        .ThrowsAsync(new KeyNotFoundException("Veículo não encontrado"));

            var result = await _controller.Delete(id);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<NotFoundObjectResult>());

                var notFound = result as NotFoundObjectResult;
                Assert.That(notFound, Is.Not.Null);

                var problem = notFound!.Value as ProblemDetails;
                Assert.That(problem, Is.Not.Null);
                Assert.That(problem!.Status, Is.EqualTo(StatusCodes.Status404NotFound));
                Assert.That(problem.Title, Is.EqualTo("Não encontrado"));
                Assert.That(problem.Detail, Is.EqualTo("Veículo não encontrado"));
            });

            _service.Verify(s => s.RemoveVehicleAsync(id), Times.Once);
        }
    }
}

