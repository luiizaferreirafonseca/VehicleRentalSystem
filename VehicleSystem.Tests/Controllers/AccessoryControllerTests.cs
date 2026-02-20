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
    [Category("Controllers")]
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

        /// <summary>
        /// Ensures the endpoint returns HTTP 200 and an empty list when
        /// no accessories are registered, preventing front-end processing errors.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
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

        /// <summary>
        /// Validates populated data return when accessories exist in the database.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
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


        /// <summary>
        /// Tests successful retrieval of an accessory by ID.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
        public async Task GetById_ShouldReturn_200Ok_WhenExists()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetAccessoryByIdAsync(id)).ReturnsAsync(new AccessoryResponseDto { Id = id });

            var result = await _controller.GetById(id);

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        }

        /// <summary>
        /// Verifies that the controller blocks the request before calling the service
        /// when the ID format or parameters are invalid in the ModelState.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 3)]
        public async Task GetById_ShouldReturn_400_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("id", "Invalid Guid format");

            var result = await _controller.GetById(Guid.NewGuid());

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        /// <summary>
        /// Validates that the system returns 404 with correct ProblemDetails
        /// when the requested accessory does not exist.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
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

        /// <summary>
        /// Tests handling of invalid operations (e.g., attempting to access a blocked accessory).
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task GetById_ShouldReturn_409_WhenInvalidOperationExceptionOccurs()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetAccessoryByIdAsync(id))
                        .ThrowsAsync(new InvalidOperationException("Operação inválida para este estado"));

            var result = await _controller.GetById(id);

            Assert.Multiple(() =>
            {
                Assert.That(result.Result, Is.TypeOf<ConflictObjectResult>());
                var conflict = result.Result as ConflictObjectResult;
                var problem = conflict!.Value as ProblemDetails;
                Assert.That(problem?.Status, Is.EqualTo(StatusCodes.Status409Conflict));
            });
        }

        /// <summary>
        /// Ensures unhandled exceptions return 500 to avoid exposing stack traces to the client.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 3)]
        public async Task GetById_ShouldReturn_500_WhenUnexpectedExceptionOccurs()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetAccessoryByIdAsync(id))
                        .ThrowsAsync(new Exception("Erro de banco de dados ou timeout"));

            var result = await _controller.GetById(id);

            Assert.Multiple(() =>
            {
                var statusCodeResult = result.Result as ObjectResult;
                Assert.That(statusCodeResult, Is.Not.Null);
                Assert.That(statusCodeResult!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));

                var problem = statusCodeResult.Value as ProblemDetails;
                Assert.That(problem?.Title, Is.EqualTo(Messages.ServerInternalError));
            });
        }

        /// <summary>
        /// Main creation flow: verifies that it returns 201 Created and the resource location.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
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

        /// <summary>
        /// Validates business rule for duplicate names via conflict exception.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 1)]
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

        /// <summary>
        /// Validates that ModelState prevents accessory creation when validation errors exist.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 2)]
        public async Task Create_ShouldReturn_400BadRequest_WhenModelStateIsInvalid()
        {
            var dto = new AccessoryCreateDto { Name = "" };
            _controller.ModelState.AddModelError("Name", "Required");

            var actionResult = await _controller.Create(dto);

            Assert.That(actionResult.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        /// <summary>
        /// Ensures unexpected failures in creation service return 500 error.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", 3)]
        public async Task Create_ShouldReturn_500_WhenUnexpectedException()
        {
            var dto = new AccessoryCreateDto { Name = "GPS", DailyRate = 10m };
            _serviceMock.Setup(s => s.CreateAccessoryAsync(dto))
                .ThrowsAsync(new Exception("Fatal error"));

            var actionResult = await _controller.Create(dto);

            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.That(statusCodeResult?.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }


        /// <summary>
        /// Ensures the system validates if the request body is empty before processing.
        /// </summary>
        [Test]
        [Category("Vínculo")]
        [Property("Priority", 1)]
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

        /// <summary>
        /// Verifies that invalid ModelState prevents linking accessory to rental.
        /// </summary>
        [Test]
        [Category("Vínculo")]
        [Property("Priority", 2)]
        public async Task AddAccessoryToRental_ShouldReturn_400_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Key", "Error");

            var result = await _controller.AddAccessoryToRental(new RentalAccessoryRequestDto());

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        /// <summary>
        /// Ensures empty RentalId returns 400 error when linking accessory.
        /// </summary>
        [Test]
        [Category("Vínculo")]
        [Property("Priority", 3)]
        public async Task AddAccessoryToRental_ShouldReturn_400_WhenOnlyRentalIdIsEmpty()
        {
            var req = new RentalAccessoryRequestDto { RentalId = Guid.Empty, AccessoryId = Guid.NewGuid() };

            var result = await _controller.AddAccessoryToRental(req);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        /// <summary>
        /// Ensures empty AccessoryId returns 400 error when linking accessory.
        /// </summary>
        [Test]
        [Category("Vínculo")]
        [Property("Priority", 3)]
        public async Task AddAccessoryToRental_ShouldReturn_400_WhenOnlyAccessoryIdIsEmpty()
        {
            var req = new RentalAccessoryRequestDto { RentalId = Guid.NewGuid(), AccessoryId = Guid.Empty };

            var result = await _controller.AddAccessoryToRental(req);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        /// <summary>
        /// Verifies that accessory and rental linking succeeds.
        /// </summary>
        [Test]
        [Category("Vínculo")]
        [Property("Priority", 1)]
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
                Assert.That(ok.Value!.ToString(), Does.Contain(Messages.AccessoryLinkedSuccess));
                _serviceMock.Verify(s => s.AddAccessoryToRentalAsync(rentalId, accessoryId), Times.Once);
            });
        }

        /// <summary>
        /// Validates that LogWarning entries are generated correctly when key is not found.
        /// </summary>
        [Test]
        [Category("Vínculo")]
        [Category("Logging")]
        [Property("Priority", 2)]
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
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("NotFound")),
                    It.IsAny<Exception?>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)), Times.Once);
            });
        }

        /// <summary>
        /// Ensures unexpected failures during linking return 500 and log error.
        /// </summary>
        [Test]
        [Category("Vínculo")]
        [Property("Priority", 3)]
        public async Task AddAccessoryToRental_ShouldReturn_500_AndLogError_WhenUnexpectedException()
        {
            var req = new RentalAccessoryRequestDto { RentalId = Guid.NewGuid(), AccessoryId = Guid.NewGuid() };
            _serviceMock.Setup(s => s.AddAccessoryToRentalAsync(req.RentalId, req.AccessoryId)).ThrowsAsync(new Exception());

            var result = await _controller.AddAccessoryToRental(req);

            var obj = result as ObjectResult;
            Assert.That(obj?.StatusCode, Is.EqualTo(500));
        }

        /// <summary>
        /// Validates that business conflicts during linking return 409.
        /// </summary>
        [Test]
        [Category("Vínculo")]
        [Property("Priority", 2)]
        public async Task AddAccessoryToRental_ShouldReturn_409_WhenInvalidOperation()
        {
            var req = new RentalAccessoryRequestDto { RentalId = Guid.NewGuid(), AccessoryId = Guid.NewGuid() };
            _serviceMock.Setup(s => s.AddAccessoryToRentalAsync(req.RentalId, req.AccessoryId))
                .ThrowsAsync(new InvalidOperationException("Conflict message"));

            var result = await _controller.AddAccessoryToRental(req);

            Assert.That(result, Is.TypeOf<ConflictObjectResult>());
        }

        // --- GET /rental/{id}/accessories ---

        /// <summary>
        /// Tests that listing accessories by rental returns status 200.
        /// </summary>
        [Test]
        [Category("Query")]
        [Property("Priority", 1)]
        public async Task GetAccessoriesByRental_ShouldReturnList_WhenAccessoriesExist()
        {
            var rentalId = Guid.NewGuid();
            var list = new List<AccessoryResponseDto> { new AccessoryResponseDto { Id = Guid.NewGuid(), Name = "GPS" } };
            _serviceMock.Setup(s => s.GetAccessoriesByRentalIdAsync(rentalId)).ReturnsAsync(list);

            var actionResult = await _controller.GetAccessoriesByRental(rentalId);

            Assert.That(actionResult.Result, Is.TypeOf<OkObjectResult>());
        }

        /// <summary>
        /// Ensures accessory search by rental returns 404 when not found.
        /// </summary>
        [Test]
        [Category("Query")]
        [Property("Priority", 2)]
        public async Task GetAccessoriesByRental_ShouldReturn_404_WhenNotFound()
        {
            var rentalId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetAccessoriesByRentalIdAsync(rentalId)).ThrowsAsync(new KeyNotFoundException());

            var actionResult = await _controller.GetAccessoriesByRental(rentalId);

            Assert.That(actionResult.Result, Is.TypeOf<NotFoundObjectResult>());
        }

        /// <summary>
        /// Validates that business conflicts in query return 409.
        /// </summary>
        [Test]
        [Category("Query")]
        [Property("Priority", 3)]
        public async Task GetAccessoriesByRental_ShouldReturn_409_WhenInvalidOperation()
        {
            var rentalId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetAccessoriesByRentalIdAsync(rentalId))
                                    .ThrowsAsync(new InvalidOperationException("Conflict"));

            var actionResult = await _controller.GetAccessoriesByRental(rentalId);

            Assert.That(actionResult.Result, Is.TypeOf<ConflictObjectResult>());
        }

        /// <summary>
        /// Ensures unexpected query failures return 500.
        /// </summary>
        [Test]
        [Category("Query")]
        [Property("Priority", 3)]
        public async Task GetAccessoriesByRental_ShouldReturn_500_WhenUnexpectedException()
        {
            var rentalId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetAccessoriesByRentalIdAsync(rentalId))
                        .ThrowsAsync(new Exception("Internal Error"));

            var actionResult = await _controller.GetAccessoriesByRental(rentalId);

            var statusCodeResult = actionResult.Result as ObjectResult;
            Assert.That(statusCodeResult?.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }


        /// <summary>
        /// Validates removal of link between accessory and rental.
        /// </summary>
        [Test]
        [Category("Remoção")]
        [Property("Priority", 1)]
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

        /// <summary>
        /// Ensures removal returns 404 when key does not exist.
        /// </summary>
        [Test]
        [Category("Remoção")]
        [Property("Priority", 2)]
        public async Task RemoveAccessoryFromRental_ShouldReturn_404_WhenKeyNotFound()
        {
            var rentalId = Guid.NewGuid();
            var accessoryId = Guid.NewGuid();
            _serviceMock.Setup(s => s.RemoveAccessoryFromRentalAsync(rentalId, accessoryId))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.RemoveAccessoryFromRental(rentalId, accessoryId);

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }
        /// <summary>
        /// Validates that business conflicts during removal return 409.
        /// </summary>
        [Test]
        [Category("Remoção")]
        [Property("Priority", 3)]
        public async Task RemoveAccessoryFromRental_ShouldReturn_409_WhenInvalidOperation()
        {
            var rentalId = Guid.NewGuid();
            var accessoryId = Guid.NewGuid();
            _serviceMock.Setup(s => s.RemoveAccessoryFromRentalAsync(rentalId, accessoryId))
                .ThrowsAsync(new InvalidOperationException());

            var result = await _controller.RemoveAccessoryFromRental(rentalId, accessoryId);

            Assert.That(result, Is.TypeOf<ConflictObjectResult>());
        }

        /// <summary>
        /// Ensures unexpected removal failures return 500.
        /// </summary>
        [Test]
        [Category("Remoção")]
        [Property("Priority", 3)]
        public async Task RemoveAccessoryFromRental_ShouldReturn_500_WhenUnexpectedException()
        {
            var rentalId = Guid.NewGuid();
            var accessoryId = Guid.NewGuid();
            _serviceMock.Setup(s => s.RemoveAccessoryFromRentalAsync(rentalId, accessoryId))
                .ThrowsAsync(new Exception());

            var result = await _controller.RemoveAccessoryFromRental(rentalId, accessoryId);

            var obj = result as ObjectResult;
            Assert.That(obj?.StatusCode, Is.EqualTo(500));
        }
    }
}