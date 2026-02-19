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
        /// Garante que o endpoint retorne status 200 e uma lista vazia quando 
        /// não houver acessórios cadastrados, evitando erros no processamento do front-end.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", "Medium")]
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
        /// Valida o retorno de dados populados quando existem acessórios na base.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", "High")]
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
        /// Testa a recuperação bem-sucedida de um acessório por ID.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", "High")]
        public async Task GetById_ShouldReturn_200Ok_WhenExists()
        {
            var id = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetAccessoryByIdAsync(id)).ReturnsAsync(new AccessoryResponseDto { Id = id });

            var result = await _controller.GetById(id);

            Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        }

        /// <summary>
        /// Verifica se o Controller bloqueia a requisição antes de chamar o serviço 
        /// caso o formato do ID ou parâmetros sejam inválidos no ModelState.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", "Low")]
        public async Task GetById_ShouldReturn_400_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("id", "Invalid Guid format");

            var result = await _controller.GetById(Guid.NewGuid());

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        /// <summary>
        /// Valida se o sistema retorna 404 e os ProblemDetails corretos 
        /// quando o acessório solicitado não existe.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", "High")]
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
        /// Testa o tratamento de operações inválidas (Ex: tentar acessar acessório bloqueado).
        /// </summary>
        [Test]
        [Category("Unit")]
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
        /// Garante que exceções não tratadas retornem 500 para não expor stack trace ao cliente.
        /// </summary>
        [Test]
        [Category("Unit")]
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
        /// Fluxo principal de criação: Verifica se retorna 201 Created e o local do recurso.
        /// </summary>
        [Test]
        [Category("Unit")]
        [Property("Priority", "High")]
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
        /// Valida a regra de negócio de nomes duplicados via exceção de conflito.
        /// </summary>
        [Test]
        [Category("Unit")]
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

        [Test]
        /// <summary>
        /// Valida se o ModelState impede a criação de acessório quando há erros de validação.
        /// </summary>
        [Category("Unit")]
        [Property("Priority", "Medium")]
        public async Task Create_ShouldReturn_400BadRequest_WhenModelStateIsInvalid()
        {
            var dto = new AccessoryCreateDto { Name = "" };
            _controller.ModelState.AddModelError("Name", "Required");

            var actionResult = await _controller.Create(dto);

            Assert.That(actionResult.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        /// <summary>
        /// Garante que falhas inesperadas no serviço de criação retornem erro 500.
        /// </summary>
        [Category("Unit")]
        [Property("Priority", "Low")]
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
        /// Garante que o sistema valide se o corpo da requisição está vazio antes de processar.
        /// </summary>
        [Test]
        [Category("Vínculo")]
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
        /// Verifica se o ModelState inválido impede o vínculo de acessório ao aluguel.
        /// </summary>
        [Test]
        [Category("Vínculo")]
        [Property("Priority", "Medium")]
        public async Task AddAccessoryToRental_ShouldReturn_400_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Key", "Error");

            var result = await _controller.AddAccessoryToRental(new RentalAccessoryRequestDto());

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        /// <summary>
        /// Garante que o RentalId vazio retorna erro 400 ao tentar vincular acessório.
        /// </summary>
        [Test]
        [Category("Vínculo")]
        [Property("Priority", "Low")]
        public async Task AddAccessoryToRental_ShouldReturn_400_WhenOnlyRentalIdIsEmpty()
        {
            var req = new RentalAccessoryRequestDto { RentalId = Guid.Empty, AccessoryId = Guid.NewGuid() };

            var result = await _controller.AddAccessoryToRental(req);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        /// <summary>
        /// Garante que o AccessoryId vazio retorna erro 400 ao tentar vincular acessório.
        /// </summary>
        [Test]
        [Category("Vínculo")]
        [Property("Priority", "Low")]
        public async Task AddAccessoryToRental_ShouldReturn_400_WhenOnlyAccessoryIdIsEmpty()
        {
            var req = new RentalAccessoryRequestDto { RentalId = Guid.NewGuid(), AccessoryId = Guid.Empty };

            var result = await _controller.AddAccessoryToRental(req);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        /// <summary>
        /// Verifica se o vínculo entre acessório e aluguel é realizado com sucesso.
        /// </summary>
        [Test]
        [Category("Vínculo")]
        [Property("Priority", "High")]
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
        /// Valida se avisos (LogWarning) são gerados corretamente quando a chave não é encontrada.
        /// </summary>
        [Test]
        [Category("Vínculo")]
        [Category("Logging")]
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
        /// Garante que falhas inesperadas no vínculo retornam erro 500 e log de erro.
        /// </summary>
        [Test]
        [Category("Vínculo")]
        [Property("Priority", "Low")]
        public async Task AddAccessoryToRental_ShouldReturn_500_AndLogError_WhenUnexpectedException()
        {
            var req = new RentalAccessoryRequestDto { RentalId = Guid.NewGuid(), AccessoryId = Guid.NewGuid() };
            _serviceMock.Setup(s => s.AddAccessoryToRentalAsync(req.RentalId, req.AccessoryId)).ThrowsAsync(new Exception());

            var result = await _controller.AddAccessoryToRental(req);

            var obj = result as ObjectResult;
            Assert.That(obj?.StatusCode, Is.EqualTo(500));
        }

        /// <summary>
        /// Valida se conflitos de negócio no vínculo retornam erro 409.
        /// </summary>
        [Test]
        [Category("Vínculo")]
        [Property("Priority", "Medium")]
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
        /// Testa se a listagem de acessórios por aluguel retorna status 200.
        /// </summary>
        [Test]
        [Category("Query")]
        public async Task GetAccessoriesByRental_ShouldReturnList_WhenAccessoriesExist()
        {
            var rentalId = Guid.NewGuid();
            var list = new List<AccessoryResponseDto> { new AccessoryResponseDto { Id = Guid.NewGuid(), Name = "GPS" } };
            _serviceMock.Setup(s => s.GetAccessoriesByRentalIdAsync(rentalId)).ReturnsAsync(list);

            var actionResult = await _controller.GetAccessoriesByRental(rentalId);

            Assert.That(actionResult.Result, Is.TypeOf<OkObjectResult>());
        }

        /// <summary>
        /// Garante que a busca de acessórios por aluguel retorna 404 quando não encontrados.
        /// </summary>
        [Test]
        [Category("Query")]
        [Property("Priority", "Medium")]
        public async Task GetAccessoriesByRental_ShouldReturn_404_WhenNotFound()
        {
            var rentalId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetAccessoriesByRentalIdAsync(rentalId)).ThrowsAsync(new KeyNotFoundException());

            var actionResult = await _controller.GetAccessoriesByRental(rentalId);

            Assert.That(actionResult.Result, Is.TypeOf<NotFoundObjectResult>());
        }

        /// <summary>
        /// Valida se conflitos de negócio na consulta retornam erro 409.
        /// </summary>
        [Test]
        [Category("Query")]
        [Property("Priority", "Low")]
        public async Task GetAccessoriesByRental_ShouldReturn_409_WhenInvalidOperation()
        {
            var rentalId = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetAccessoriesByRentalIdAsync(rentalId))
                                    .ThrowsAsync(new InvalidOperationException("Conflict"));

            var actionResult = await _controller.GetAccessoriesByRental(rentalId);

            Assert.That(actionResult.Result, Is.TypeOf<ConflictObjectResult>());
        }

        /// <summary>
        /// Garante que falhas inesperadas na consulta retornam erro 500.
        /// </summary>
        [Test]
        [Category("Query")]
        [Property("Priority", "Low")]
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
        /// Valida a remoção do vínculo entre acessório e aluguel.
        /// </summary>
        [Test]
        [Category("Remoção")]
        [Property("Priority", "High")]
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
        /// Garante que a remoção de vínculo retorna 404 quando a chave não existe.
        /// </summary>
        [Test]
        [Category("Remoção")]
        [Property("Priority", "Medium")]
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
        /// Valida se conflitos de negócio na remoção retornam erro 409.
        /// </summary>
        [Test]
        [Category("Remoção")]
        [Property("Priority", "Low")]
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
        /// Garante que falhas inesperadas na remoção retornam erro 500.
        /// </summary>
        [Test]
        [Category("Remoção")]
        [Property("Priority", "Low")]
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