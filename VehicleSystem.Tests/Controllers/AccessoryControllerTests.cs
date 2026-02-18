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
    }
}