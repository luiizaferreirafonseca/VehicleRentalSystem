using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Services.interfaces;
using API_SistemaLocacao.Controllers; 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VehicleSystem.Tests.Controllers
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<IUserService> _serviceMock;
        private UserController _controller;

        [SetUp]
        public void SetUp()
        {
            _serviceMock = new Mock<IUserService>();
            _controller = new UserController(_serviceMock.Object);
        }


        [Test]
        public async Task Post_QuandoDadosValidos_DeveRetornar201Created()
        {
            var request = new UserCreateDTO { Name = "Ale Teste", Email = "ale@teste.com" };
            var response = new UserResponseDTO { Id = Guid.NewGuid(), Name = "Ale Teste", Email = "ale@teste.com" };

            _serviceMock.Setup(x => x.CreateUserAsync(request)).ReturnsAsync(response);

            var result = await _controller.Post(request);

            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
            var createdResult = result as CreatedAtActionResult;
            Assert.That(createdResult!.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
        }

        [Test]
        public async Task Post_QuandoEmailJaExiste_DeveRetornar400BadRequest()
        {
            var request = new UserCreateDTO { Name = "Ale", Email = "jaexiste@teste.com" };

            _serviceMock.Setup(x => x.CreateUserAsync(It.IsAny<UserCreateDTO>()))
                .ThrowsAsync(new InvalidOperationException("Este e-mail já está cadastrado no sistema."));

            var result = await _controller.Post(request);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequest = result as BadRequestObjectResult;
            var problem = badRequest!.Value as ProblemDetails;
            Assert.That(problem!.Detail, Is.EqualTo("Este e-mail já está cadastrado no sistema."));
        }


        [Test]
        public async Task Get_QuandoExistiremUsuarios_DeveRetornar200Ok()
        {
            // Arrange
            var listaUsuarios = new List<UserResponseDTO> 
            { 
                new UserResponseDTO { Name = "Ale", Email = "ale@teste.com" } 
            };
            
            _serviceMock.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(listaUsuarios);

            // Act 
            var result = await _controller.Get();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        [Test]
        public async Task Get_QuandoOcorrerErro_DeveRetornar500InternalServerError()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetAllUsersAsync())
                .ThrowsAsync(new Exception("Erro genérico"));

            // Act 
            var result = await _controller.Get();

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult!.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }
    }
}