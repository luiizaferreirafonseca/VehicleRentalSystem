using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Services.interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleRentalSystem.Controllers;

namespace VehicleSystem.Tests.Controllers
{
    [TestFixture]
    public class VehicleControllerTests
    {
        private Mock<IVehicleService> _service = null!;
        private VehicleController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<IVehicleService>();
            _controller = new VehicleController(_service.Object);
        }

        [Test]
        public async Task CreateVehicle_ServiceRetornaSucesso_DeveRetornar201ComBody()
        {
            var dto = new VehicleCreateDTO { Brand = "Chevrolet", Model = "Onix", Year = 2022, DailyRate = 150m, LicensePlate = "ABC1234" };
            var created = new VehicleResponseDTO { Id = Guid.NewGuid(), Brand = dto.Brand, Status = "available" };

            _service.Setup(s => s.CreateVehicleAsync(dto)).ReturnsAsync(created);
            var result = await _controller.Create(dto);

            Assert.That(result, Is.TypeOf<ObjectResult>());
            var obj = result as ObjectResult;
            Assert.That(obj?.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
        }

        [Test]
        public async Task Delete_ShouldReturn_204NoContent_WhenRemovedSuccessfully()
        {
            var id = Guid.NewGuid();
            _service.Setup(s => s.RemoveVehicleAsync(id)).Returns(Task.CompletedTask);
            var result = await _controller.Delete(id);
            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task Update_ShouldReturn_200Ok_WhenUpdateIsSuccessful()
        {
            var vehicleId = Guid.NewGuid();
            var dto = new VehicleUpdateDTO { DailyRate = 200m, Year = 2025, Status = "rented" };
            var response = new VehicleResponseDTO { Id = vehicleId, DailyRate = 200m, Year = 2025, Status = "rented" };

            _service.Setup(s => s.UpdateVehicleAsync(vehicleId, dto)).ReturnsAsync(response);
            var result = await _controller.Update(vehicleId, dto);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            var body = ok?.Value as VehicleResponseDTO;
            Assert.That(body?.DailyRate, Is.EqualTo(200m));
        }

        [Test]
        public async Task Update_ShouldReturn_404NotFound_WhenVehicleDoesNotExist()
        {
            var vehicleId = Guid.NewGuid();
            var dto = new VehicleUpdateDTO { DailyRate = 200m };
            _service.Setup(s => s.UpdateVehicleAsync(vehicleId, dto)).ThrowsAsync(new KeyNotFoundException("Not Found"));

            var result = await _controller.Update(vehicleId, dto);
            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }

        // ─── GetAvailable ────────────────────────────────────────────────────────

        [Test]
        public async Task GetAvailable_ServicoRetornaVeiculos_Retorna200ComCorpo()
        {
            var response = new VehicleListResponseDTO
            {
                Vehicles = new List<VehicleResponseDTO>
                {
                    new() { Id = Guid.NewGuid(), Brand = "Toyota", Model = "Corolla", Status = "available" },
                    new() { Id = Guid.NewGuid(), Brand = "Honda",  Model = "Civic",   Status = "available" }
                }
            };

            _service.Setup(s => s.GetAvailableVehiclesAsync(1)).ReturnsAsync(response);

            var result = await _controller.GetAvailable();

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            var body = ok?.Value as VehicleListResponseDTO;
            Assert.That(body?.Vehicles, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task GetAvailable_ListaVazia_Retorna200ComListaVazia()
        {
            var response = new VehicleListResponseDTO { Vehicles = new List<VehicleResponseDTO>() };

            _service.Setup(s => s.GetAvailableVehiclesAsync(1)).ReturnsAsync(response);

            var result = await _controller.GetAvailable();

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;
            var body = ok?.Value as VehicleListResponseDTO;
            Assert.That(body?.Vehicles, Is.Empty);
        }

        [Test]
        public async Task GetAvailable_PaginaCustomizada_RepassaPageAoServico()
        {
            var response = new VehicleListResponseDTO
            {
                Vehicles = new List<VehicleResponseDTO> { new() { Id = Guid.NewGuid(), Status = "available" } }
            };

            _service.Setup(s => s.GetAvailableVehiclesAsync(3)).ReturnsAsync(response);

            var result = await _controller.GetAvailable(page: 3);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            _service.Verify(s => s.GetAvailableVehiclesAsync(3), Times.Once);
        }

        [Test]
        public async Task GetAvailable_LancaExcecao_Retorna500()
        {
            _service.Setup(s => s.GetAvailableVehiclesAsync(1))
                .ThrowsAsync(new Exception("Erro inesperado"));

            var result = await _controller.GetAvailable();

            Assert.That(result, Is.TypeOf<ObjectResult>());
            var obj = result as ObjectResult;
            Assert.That(obj?.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }
    }
}