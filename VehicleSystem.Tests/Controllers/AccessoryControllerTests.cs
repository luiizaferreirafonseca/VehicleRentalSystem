using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Moq;
using API_SistemaLocacao.Controllers;
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
            // Arrange: Inicialização das dependências mockadas
            _serviceMock = new Mock<IAccessoryService>();
            _loggerMock = new Mock<ILogger<AccessoryController>>();

            // Instanciação do Controller injetando os objetos simulados
            _controller = new AccessoryController(_serviceMock.Object, _loggerMock.Object);
        }

        // --- CENÁRIO: LISTA VAZIA MÉTODO GET ---
        [Test]
        public async Task GetAccessories_ShouldReturn_200OkWithEmptyList_WhenNoAccessoriesExist()
        {
            // Arrange: Configura o serviço para retornar uma lista sem itens
            _serviceMock.Setup(s => s.GetAccessoriesAsync())
                        .ReturnsAsync(new List<AccessoryResponseDto>());

            // Act: Chama o método Get do Controller
            var actionResult = await _controller.Get();

            // Assert: Agrupa validações de tipo e conteúdo
            Assert.Multiple(() =>
            {
                // Verifica se o resultado é do tipo OkObjectResult (Status 200)
                Assert.That(actionResult.Result, Is.TypeOf<OkObjectResult>());

                var ok = actionResult.Result as OkObjectResult;
                var list = ok?.Value as IEnumerable<AccessoryResponseDto>;

                // Verifica se a lista contida no retorno está vazia
                Assert.That(list, Is.Empty);
            });
        }

        // --- CENÁRIO: LISTA COM ITENS ---
        [Test]
        public async Task GetAccessories_ShouldReturnList_WhenAccessoriesExist()
        {
            // Arrange: Criação de dados fictícios para a simulação
            var fakeAccessories = new List<AccessoryResponseDto>
            {
                new AccessoryResponseDto { Id = Guid.NewGuid(), Name = "GPS", DailyRate = 15.0m },
                new AccessoryResponseDto { Id = Guid.NewGuid(), Name = "Cadeirinha", DailyRate = 30.0m }
            };

            _serviceMock.Setup(serv => serv.GetAccessoriesAsync())
                        .ReturnsAsync(fakeAccessories);

            // Act: Aciona a busca de acessórios
            var actionResult = await _controller.Get();

            // Assert: Verifica se os dados retornados condizem com o Mock
            Assert.Multiple(() =>
            {
                var ok = actionResult.Result as OkObjectResult;
                Assert.That(ok, Is.Not.Null);

                var result = (ok!.Value as IEnumerable<AccessoryResponseDto>)?.ToList();

                Assert.That(result, Is.Not.Null);
                Assert.That(result!.Count, Is.EqualTo(2));
                Assert.That(result[0].Name, Is.EqualTo("GPS"));
                Assert.That(result[0].DailyRate, Is.EqualTo(15.0m));
            });
        }

        // --- CENÁRIO: BUSCA POR ID INEXISTENTE ---
        [Test]
        public async Task GetById_ShouldReturn_404NotFound_WhenIdDoesNotExist()
        {
            // Arrange : Configura o mock para lançar KeyNotFoundException
            var idInexistente = Guid.NewGuid();
            _serviceMock.Setup(s => s.GetAccessoryByIdAsync(idInexistente))
                        .ThrowsAsync(new KeyNotFoundException("Acessório não encontrado"));

            // Act : Chama o método GetById do controller
            var actionResult = await _controller.GetById(idInexistente);

            // Assert : Verifica se retornou 404 e os detalhes do problema
            Assert.Multiple(() =>
            {
                Assert.That(actionResult.Result, Is.TypeOf<NotFoundObjectResult>());

                var response = actionResult.Result as NotFoundObjectResult;
                var problem = response?.Value as ProblemDetails;

                Assert.That(problem?.Status, Is.EqualTo(StatusCodes.Status404NotFound));
                Assert.That(problem?.Title, Is.EqualTo(Messages.NotFound));
            });
        }

        // --- CENÁRIO: CONFLITO DE NOME DUPLICADO NO CADASTRO ---
        [Test]
        public async Task Create_ShouldReturn_409Conflict_WhenNameIsDuplicate()
        {
            // Arrange : Configura o serviço para lançar InvalidOperationException
            var dto = new AccessoryCreateDto { Name = "GPS", DailyRate = 10m };
            _serviceMock.Setup(s => s.CreateAccessoryAsync(dto))
                        .ThrowsAsync(new InvalidOperationException("Acessório já cadastrado"));

            // Act : Tenta criar um acessório com nome já existente
            var actionResult = await _controller.Create(dto);

            // Assert : Verifica se retornou 409 Conflict
            Assert.Multiple(() =>
            {
                Assert.That(actionResult.Result, Is.TypeOf<ConflictObjectResult>());

                var response = actionResult.Result as ConflictObjectResult;
                var problem = response?.Value as ProblemDetails;

                Assert.That(problem?.Status, Is.EqualTo(StatusCodes.Status409Conflict));
                Assert.That(problem?.Title, Is.EqualTo(Messages.Conflict));
            });
        }

        // --- CENÁRIO: VALIDAÇÃO DE MODELO INVÁLIDA ---
        [Test]
        public async Task Create_ShouldReturn_400BadRequest_WhenModelStateIsInvalid()
        {
            // Arrange : Adiciona erro manual ao ModelState para simular falha de validação
            var dtoIncompleto = new AccessoryCreateDto { Name = "" }; // Nome vazio
            _controller.ModelState.AddModelError("Name", "O nome é obrigatório");

            // Act : Chama o método Create
            var actionResult = await _controller.Create(dtoIncompleto);

            // Assert : Verifica se retornou 400 BadRequest
            Assert.That(actionResult.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        //  --- CENÁRIO: VALIDAÇÃO DE RETORNO PARA BODY VAZIO
        [Test]
        public async Task AddAccessoryToRental_ShouldReturn_400BadRequest_WhenRequestIsNull()
        {
            // Arrange : Define o objeto de requisição como nulo
            RentalAccessoryRequestDto? request = null;

            // Act : Chama o método passando o valor nulo
            // Usamos o operador ! para suprimir o aviso de nulo, pois queremos testar justamente esse comportamento
            var result = await _controller.AddAccessoryToRental(request!);

            // Assert : Verifica se o retorno é um BadRequest com ProblemDetails
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.TypeOf<BadRequestObjectResult>());

                var response = result as BadRequestObjectResult;
                var problem = response?.Value as ProblemDetails;

                Assert.That(problem?.Status, Is.EqualTo(StatusCodes.Status400BadRequest));
                Assert.That(problem?.Title, Is.EqualTo(Messages.RequestInvalid));
            });
        }

        // --- CENÁRIO: CADASTRO DE ACESSÓRIO COM SUCESSO
        [Test]
        public async Task Create_ShouldReturn_201Created_WhenAccessoryCreatedSuccessfully()
        {
            // Arrange : Configura o DTO de entrada e o retorno esperado com ID
            var request = new AccessoryCreateDto { Name = "GPS", DailyRate = 15.0m };
            var createdResponse = new AccessoryResponseDto
            {
                Id = Guid.NewGuid(),
                Name = "GPS",
                DailyRate = 15.0m
            };

            _serviceMock.Setup(s => s.CreateAccessoryAsync(request))
                        .ReturnsAsync(createdResponse);

            // Act : Chama o método Create do controller
            var actionResult = await _controller.Create(request);

            // Assert : Verifica status 201 e se aponta para a rota de busca por ID
            Assert.Multiple(() =>
            {
                Assert.That(actionResult.Result, Is.TypeOf<CreatedAtActionResult>());

                var createdResult = actionResult.Result as CreatedAtActionResult;
                Assert.That(createdResult!.StatusCode, Is.EqualTo(StatusCodes.Status201Created));

                // Verifica se o objeto retornado no corpo é o que o serviço criou
                var resultData = createdResult.Value as AccessoryResponseDto;
                Assert.That(resultData!.Id, Is.EqualTo(createdResponse.Id));
                Assert.That(resultData.Name, Is.EqualTo("GPS"));
            });
        }        
    }
}