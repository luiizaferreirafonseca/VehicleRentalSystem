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
            _serviceMock = new Mock<IAccessoryService>();
            _loggerMock = new Mock<ILogger<AccessoryController>>();
            _controller = new AccessoryController(_serviceMock.Object, _loggerMock.Object);
        }

        // --- CENÁRIO: LISTA VAZIA  MÉTODO GET ---
        [Test]
        public async Task GetAccessories_ShouldReturn_200Ok_WhenNoAccessoriesExist()
        {
            // Arrange: Simula retorno de lista vazia pelo serviço
            _serviceMock.Setup(s => s.GetAccessoriesAsync())
                        .ReturnsAsync(new List<AccessoryResponseDto>());

            // Act: Chama o método Get do Controller
            var actionResult = await _controller.Get();

            // Assert: Verifica se o status retornado é 200 OK
            Assert.That(actionResult, Is.Not.Null);
            Assert.That(actionResult.Result, Is.TypeOf<OkObjectResult>());
        }

         }
}