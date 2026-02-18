using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Moq;
using VehicleRentalSystem.Controllers;
using Microsoft.AspNetCore.Mvc;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Services.interfaces;
using Microsoft.AspNetCore.Http;
using VehicleRentalSystem.Resources;

namespace VehicleSystem.Tests.Controllers
{
    [TestFixture]
    public class AccessoryControllerTests
    {
        private Mock<IAccessoryService> _serviceMock;
        private Mock<ILogger<AccessoryController>> _loggerMock;
        private AccessoryController _controller;

        [SetUp]
        public void SetUp()
        {
            _serviceMock = new Mock<IAccessoryService>();
            _loggerMock = new Mock<ILogger<AccessoryController>>();
            _controller = new AccessoryController(_serviceMock.Object, _loggerMock.Object);
        }

        // --- CENÁRIO: GET /accessories - lista vazia ---
        [Test]
        public async Task Get_ShouldReturn_200OkWithEmptyList_WhenNoAccessoriesExist()
        {
            _serviceMock.Setup(s => s.GetAccessoriesAsync()).ReturnsAsync(new List<AccessoryResponseDto>());

            var actionResult = await _controller.Get();

            Assert.Multiple(() =>
            {
                Assert.That(actionResult.Result, Is.TypeOf<OkObjectResult>());
                var ok = actionResult.Result as OkObjectResult;
                var list = ok?.Value as IEnumerable<AccessoryResponseDto>;
                Assert.That(list, Is.Empty);
            });
        }

        // --- CENÁRIO: GET /accessories - lista com itens ---
        [Test]
        public async Task Get_ShouldReturnList_WhenAccessoriesExist()
        {
            var fake = new List<AccessoryResponseDto>
            {
                new AccessoryResponseDto { Id = Guid.NewGuid(), Name = "GPS", DailyRate = 15m },
                new AccessoryResponseDto { Id = Guid.NewGuid(), Name = "Cadeirinha", DailyRate = 30m }
            };
            _serviceMock.Setup(s => s.GetAccessoriesAsync()).ReturnsAsync(fake);

            var actionResult = await _controller.Get();

            var ok = actionResult.Result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            var result = (ok!.Value as IEnumerable<AccessoryResponseDto>)?.ToList();
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Count, Is.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("GPS"));
        }

        // --- CENÁRIO: GET /accessories/{id} - encontrado ---
        [Test]
        public async Task GetById_ShouldReturn_200Ok_WhenExists()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetAccessoryByIdAsync(id)).ReturnsAsync(new AccessoryResponseDto { Id = id });

            var result = await _controller.GetById(id);

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        }

        // --- CENÁRIO: GET /accessories/{id} - não encontrado ---
        [Test]
        public async Task GetById_ShouldReturn_404NotFound_WhenIdDoesNotExist()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetAccessoryByIdAsync(id)).ThrowsAsync(new KeyNotFoundException("Not found"));

            var actionResult = await _controller.GetById(id);

            Assert.Multiple(() =>
            {
                Assert.That(actionResult.Result, Is.TypeOf<NotFoundObjectResult>());
                var response = actionResult.Result as NotFoundObjectResult;
                var problem = response?.Value as ProblemDetails;
                Assert.That(problem?.Status, Is.EqualTo(StatusCodes.Status404NotFound));
                Assert.That(problem?.Title, Is.EqualTo(Messages.NotFound));
            });
        }

        // --- CENÁRIO: POST /accessories/add - sucesso (201 Created) ---
        [Test]
        public async Task Create_ShouldReturn_201Created_WhenAccessoryCreatedSuccessfully()
        {
            var request = new AccessoryCreateDto { Name = "GPS", DailyRate = 15m };
            var created = new AccessoryResponseDto { Id = Guid.NewGuid(), Name = "GPS", DailyRate = 15m };
            _serviceMock.Setup(s => s.CreateAccessoryAsync(request)).ReturnsAsync(created);

            var actionResult = await _controller.Create(request);

            Assert.Multiple(() =>
            {
                Assert.That(actionResult.Result, Is.TypeOf<CreatedAtActionResult>());
                var createdResult = actionResult.Result as CreatedAtActionResult;
                Assert.That(createdResult!.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
                var data = createdResult.Value as AccessoryResponseDto;
                Assert.That(data!.Id, Is.EqualTo(created.Id));
            });
        }

        // --- CENÁRIO: POST /accessories/add - conflito (409) ---
        [Test]
        public async Task Create_ShouldReturn_409Conflict_WhenNameIsDuplicate()
        {
            var dto = new AccessoryCreateDto { Name = "GPS", DailyRate = 10m };
            _serviceMock.Setup(s => s.CreateAccessoryAsync(dto)).ThrowsAsync(new InvalidOperationException("Already"));

            var actionResult = await _controller.Create(dto);

            Assert.Multiple(() =>
            {
                Assert.That(actionResult.Result, Is.TypeOf<ConflictObjectResult>());
                var response = actionResult.Result as ConflictObjectResult;
                var problem = response?.Value as ProblemDetails;
                Assert.That(problem?.Status, Is.EqualTo(StatusCodes.Status409Conflict));
                Assert.That(problem?.Title, Is.EqualTo(Messages.Conflict));
            });
        }

        // --- CENÁRIO: POST /accessories/add - ModelState inválido (400) ---
        [Test]
        public async Task Create_ShouldReturn_400BadRequest_WhenModelStateIsInvalid()
        {
            var dto = new AccessoryCreateDto { Name = "" };
            _controller.ModelState.AddModelError("Name", "Required");

            var actionResult = await _controller.Create(dto);

            Assert.That(actionResult.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        // --- CENÁRIO: POST /accessories - request nulo ---
        [Test]
        public async Task AddAccessoryToRental_ShouldReturn_400BadRequest_WhenRequestIsNull()
        {
            RentalAccessoryRequestDto? request = null;

            var result = await _controller.AddAccessoryToRental(request!);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
                var response = result as BadRequestObjectResult;
                var problem = response?.Value as ProblemDetails;
                Assert.That(problem?.Status, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(problem?.Title, Is.EqualTo(Messages.RequestInvalid));
            });
        }

        // --- CENÁRIO: POST /accessories - ModelState inválido ---
        [Test]
        public async Task AddAccessoryToRental_ShouldReturn_400_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Key", "Error");

            var result = await _controller.AddAccessoryToRental(new RentalAccessoryRequestDto());

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        // --- CENÁRIO: POST /accessories - ids vazios ---
        [Test]
        public async Task AddAccessoryToRental_ShouldReturn_400_WhenOnlyRentalIdIsEmpty()
        {
            var req = new RentalAccessoryRequestDto { RentalId = Guid.Empty, AccessoryId = Guid.NewGuid() };

            var result = await _controller.AddAccessoryToRental(req);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task AddAccessoryToRental_ShouldReturn_400_WhenOnlyAccessoryIdIsEmpty()
        {
            var req = new RentalAccessoryRequestDto { RentalId = Guid.NewGuid(), AccessoryId = Guid.Empty };

            var result = await _controller.AddAccessoryToRental(req);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        // --- CENÁRIO: POST /accessories - sucesso e verificação do serviço ---
        [Test]
        public async Task AddAccessoryToRental_ShouldReturn_200Ok_WhenRequestIsValid()
        {
            var rentalId = Guid.NewGuid();
            var accessoryId = Guid.NewGuid();
            var request = new RentalAccessoryRequestDto { RentalId = rentalId, AccessoryId = accessoryId };

            _serviceMock.Setup(s => s.AddAccessoryToRentalAsync(rentalId, accessoryId)).Returns(Task.CompletedTask);

            var result = await _controller.AddAccessoryToRental(request);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<OkObjectResult>());
                var ok = result as OkObjectResult;
                Assert.That(ok!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
                Assert.That(ok.Value.ToString(), Does.Contain(Messages.AccessoryLinkedSuccess));
                _serviceMock.Verify(s => s.AddAccessoryToRentalAsync(rentalId, accessoryId), Times.Once);
            });
        }

        // --- CENÁRIO: POST /accessories - KeyNotFound -> 404 + log Warning ---
        [Test]
        public async Task AddAccessoryToRental_ShouldReturn_404_AndLogWarning_WhenKeyNotFound()
        {
            var req = new RentalAccessoryRequestDto { RentalId = Guid.NewGuid(), AccessoryId = Guid.NewGuid() };
            _serviceMock.Setup(s => s.AddAccessoryToRentalAsync(req.RentalId, req.AccessoryId)).ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.AddAccessoryToRental(req);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
                _loggerMock.Verify(l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString().Contains("NotFound")),
                    It.IsAny<Exception?>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.Once);
            });
        }

        // --- CENÁRIO: POST /accessories - exceção inesperada -> 500 ---
        [Test]
        public async Task AddAccessoryToRental_ShouldReturn_500_AndLogError_WhenUnexpectedException()
        {
            var req = new RentalAccessoryRequestDto { RentalId = Guid.NewGuid(), AccessoryId = Guid.NewGuid() };
            _serviceMock.Setup(s => s.AddAccessoryToRentalAsync(req.RentalId, req.AccessoryId)).ThrowsAsync(new Exception());

            var result = await _controller.AddAccessoryToRental(req);

            var obj = result as ObjectResult;
            Assert.That(obj?.StatusCode, Is.EqualTo(500));
        }

        // --- CENÁRIO: GET /rental/{id}/accessories - sucesso ---
        [Test]
        public async Task GetAccessoriesByRental_ShouldReturnList_WhenAccessoriesExist()
        {
            var rentalId = Guid.NewGuid();
            var list = new List<AccessoryResponseDto> { new AccessoryResponseDto { Id = Guid.NewGuid(), Name = "GPS" } };
            _serviceMock.Setup(s => s.GetAccessoriesByRentalIdAsync(rentalId)).ReturnsAsync(list);

            var actionResult = await _controller.GetAccessoriesByRental(rentalId);

            Assert.That(actionResult.Result, Is.TypeOf<OkObjectResult>());
        }

        // --- CENÁRIO: GET /rental/{id}/accessories - não encontrado ---
        [Test]
        public async Task GetAccessoriesByRental_ShouldReturn_404_WhenNotFound()
        {
            var rentalId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetAccessoriesByRentalIdAsync(rentalId)).ThrowsAsync(new KeyNotFoundException());

            var actionResult = await _controller.GetAccessoriesByRental(rentalId);

            Assert.That(actionResult.Result, Is.TypeOf<NotFoundObjectResult>());
        }

        // --- CENÁRIO: DELETE /rental/{rentalId}/accessories/{accessoryId} - sucesso ---
        [Test]
        public async Task RemoveAccessoryFromRental_ShouldReturn_200Ok()
        {
            var rentalId = Guid.NewGuid();
            var accessoryId = Guid.NewGuid();
            _serviceMock.Setup(s => s.RemoveAccessoryFromRentalAsync(rentalId, accessoryId)).Returns(Task.CompletedTask);

            var result = await _controller.RemoveAccessoryFromRental(rentalId, accessoryId);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<OkObjectResult>());
                var ok = result as OkObjectResult;
                Assert.That(ok!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
                _serviceMock.Verify(s => s.RemoveAccessoryFromRentalAsync(rentalId, accessoryId), Times.Once);
            });
        }
    }
}
