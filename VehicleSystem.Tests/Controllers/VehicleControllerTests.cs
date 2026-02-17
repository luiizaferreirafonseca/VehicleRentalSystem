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
    }
}
