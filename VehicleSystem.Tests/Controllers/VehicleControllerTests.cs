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
        public async Task Create_ShouldReturn_400BadRequest_WhenModelStateIsInvalid()
        {
            var dto = new VehicleCreateDTO();
            _controller.ModelState.AddModelError("Brand", "Required");

            var result = await _controller.Create(dto);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Create_ShouldReturn_409Conflict_WhenInvalidOperationExceptionIsThrown()
        {
            var dto = new VehicleCreateDTO { Brand = "Chevrolet", Model = "Onix", Year = 2022, DailyRate = 150m, LicensePlate = "ABC1234" };

            _service.Setup(s => s.CreateVehicleAsync(dto))
                    .ThrowsAsync(new InvalidOperationException("License plate already exists"));

            var result = await _controller.Create(dto);

            Assert.That(result, Is.TypeOf<ConflictObjectResult>());
            var conflict = result as ConflictObjectResult;

            Assert.That(conflict?.StatusCode, Is.EqualTo(StatusCodes.Status409Conflict));
            Assert.That(conflict?.Value, Is.TypeOf<ProblemDetails>());

            var problem = conflict?.Value as ProblemDetails;
            Assert.That(problem?.Status, Is.EqualTo(StatusCodes.Status409Conflict));
            Assert.That(problem?.Title, Is.EqualTo("Conflict"));
            Assert.That(problem?.Detail, Is.EqualTo("License plate already exists"));
        }

        [Test]
        public async Task Create_ShouldReturn_500InternalServerError_WhenUnhandledExceptionIsThrown()
        {
            var dto = new VehicleCreateDTO { Brand = "Chevrolet", Model = "Onix", Year = 2022, DailyRate = 150m, LicensePlate = "ABC1234" };

            _service.Setup(s => s.CreateVehicleAsync(dto))
                    .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.Create(dto);

            Assert.That(result, Is.TypeOf<ObjectResult>());
            var obj = result as ObjectResult;

            Assert.That(obj?.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(obj?.Value, Is.TypeOf<ProblemDetails>());

            var problem = obj?.Value as ProblemDetails;
            Assert.That(problem?.Status, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(problem?.Title, Is.EqualTo("Internal server error"));
            Assert.That(problem?.Detail, Is.EqualTo("Unexpected error"));
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
        public async Task Delete_ShouldReturn_400BadRequest_WhenInvalidOperationExceptionIsThrown()
        {
            var id = Guid.NewGuid();

            _service.Setup(s => s.RemoveVehicleAsync(id))
                    .ThrowsAsync(new InvalidOperationException("Invalid operation"));

            var result = await _controller.Delete(id);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = result as BadRequestObjectResult;

            Assert.That(badRequest?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(badRequest?.Value, Is.TypeOf<ProblemDetails>());

            var problem = badRequest?.Value as ProblemDetails;
            Assert.That(problem?.Status, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(problem?.Title, Is.EqualTo("Invalid operation"));
            Assert.That(problem?.Detail, Is.EqualTo("Invalid operation"));
        }

        [Test]
        public async Task Delete_ShouldReturn_404NotFound_WhenKeyNotFoundExceptionIsThrown()
        {
            var id = Guid.NewGuid();

            _service.Setup(s => s.RemoveVehicleAsync(id))
                    .ThrowsAsync(new KeyNotFoundException("Vehicle not found"));

            var result = await _controller.Delete(id);

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            var notFound = result as NotFoundObjectResult;

            Assert.That(notFound?.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
            Assert.That(notFound?.Value, Is.TypeOf<ProblemDetails>());

            var problem = notFound?.Value as ProblemDetails;
            Assert.That(problem?.Status, Is.EqualTo(StatusCodes.Status404NotFound));
            Assert.That(problem?.Title, Is.EqualTo("Not found"));
            Assert.That(problem?.Detail, Is.EqualTo("Vehicle not found"));
        }

        [Test]
        public async Task Delete_ShouldReturn_500InternalServerError_WhenUnhandledExceptionIsThrown()
        {
            var id = Guid.NewGuid();

            _service.Setup(s => s.RemoveVehicleAsync(id))
                    .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.Delete(id);

            Assert.That(result, Is.TypeOf<ObjectResult>());
            var obj = result as ObjectResult;

            Assert.That(obj?.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(obj?.Value, Is.TypeOf<ProblemDetails>());

            var problem = obj?.Value as ProblemDetails;
            Assert.That(problem?.Status, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(problem?.Title, Is.EqualTo("Internal server error"));
            Assert.That(problem?.Detail, Is.EqualTo("Unexpected error"));
        }

        [Test]
        public async Task Update_ShouldReturn_500InternalServerError_WhenUnhandledExceptionIsThrown()
        {
            var id = Guid.NewGuid();
            var dto = new VehicleUpdateDTO { DailyRate = 200m, Year = 2025, Status = "rented" };

            _service.Setup(s => s.UpdateVehicleAsync(id, dto))
                    .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.Update(id, dto);

            Assert.That(result, Is.TypeOf<ObjectResult>());
            var obj = result as ObjectResult;

            Assert.That(obj?.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(obj?.Value, Is.TypeOf<ProblemDetails>());

            var problem = obj?.Value as ProblemDetails;
            Assert.That(problem?.Status, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(problem?.Title, Is.EqualTo("Internal server error"));
            Assert.That(problem?.Detail, Is.EqualTo("Unexpected error"));
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

        [Test]
        public async Task Search_ShouldReturn200WithBody_WhenServiceReturnsVehicles()
        {
            var status = "available";
            var page = 1;

            var response = new List<VehicleResponseDTO>
            {
                new() { Id = Guid.NewGuid(), Brand = "Toyota", Model = "Corolla", Status = "available" },
                new() { Id = Guid.NewGuid(), Brand = "Honda",  Model = "Civic",   Status = "available" }
            };

            _service.Setup(s => s.SearchVehiclesAsync(status, page))
                    .ReturnsAsync(response);

            var result = await _controller.Search(status, page);

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;

            Assert.That(ok?.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(ok?.Value, Is.TypeOf<List<VehicleResponseDTO>>());

            var body = ok?.Value as List<VehicleResponseDTO>;
            Assert.That(body, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task Search_ShouldReturn400WithProblemDetails_WhenInvalidOperationExceptionIsThrown()
        {
            var status = "invalid";
            var page = 1;

            _service.Setup(s => s.SearchVehiclesAsync(status, page))
                    .ThrowsAsync(new InvalidOperationException("Invalid page"));

            var result = await _controller.Search(status, page);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = result as BadRequestObjectResult;

            Assert.That(badRequest?.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(badRequest?.Value, Is.TypeOf<ProblemDetails>());

            var problem = badRequest?.Value as ProblemDetails;
            Assert.That(problem?.Status, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(problem?.Title, Is.EqualTo("Invalid operation"));
            Assert.That(problem?.Detail, Is.EqualTo("Invalid page"));
        }

        [Test]
        public async Task Search_ShouldReturn500WithProblemDetails_WhenUnhandledExceptionIsThrown()
        {
            var status = "available";
            var page = 1;

            _service.Setup(s => s.SearchVehiclesAsync(status, page))
                    .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.Search(status, page);

            Assert.That(result, Is.TypeOf<ObjectResult>());
            var obj = result as ObjectResult;

            Assert.That(obj?.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(obj?.Value, Is.TypeOf<ProblemDetails>());

            var problem = obj?.Value as ProblemDetails;
            Assert.That(problem?.Status, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(problem?.Title, Is.EqualTo("Internal server error"));
            Assert.That(problem?.Detail, Is.EqualTo("Unexpected error"));
        }

        [Test]
        public async Task Search_ShouldUseDefaultPageOne_WhenPageIsNotProvided()
        {
            var status = "available";

            _service.Setup(s => s.SearchVehiclesAsync(status, 1))
                    .ReturnsAsync(new List<VehicleResponseDTO>());

            var result = await _controller.Search(status); 

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            _service.Verify(s => s.SearchVehiclesAsync(status, 1), Times.Once);
        }
    }
}