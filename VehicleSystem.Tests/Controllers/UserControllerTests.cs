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
        public async Task Get_ShouldReturn200Ok_WithBody_WhenServiceReturnsUsers()
        {
            var response = new List<UserResponseDTO>
            {
                new() { Id = Guid.NewGuid(), Name = "User 1" },
                new() { Id = Guid.NewGuid(), Name = "User 2" }
            };

            _serviceMock.Setup(s => s.GetAllUsersAsync())
                        .ReturnsAsync(response);

            var result = await _controller.Get();

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;

            Assert.That(ok?.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(ok?.Value, Is.TypeOf<List<UserResponseDTO>>());

            var body = ok?.Value as List<UserResponseDTO>;
            Assert.That(body, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task Get_ShouldReturn200Ok_WithEmptyList_WhenServiceReturnsEmptyList()
        {
            var response = new List<UserResponseDTO>();

            _serviceMock.Setup(s => s.GetAllUsersAsync())
                        .ReturnsAsync(response);

            var result = await _controller.Get();

            Assert.That(result, Is.TypeOf<OkObjectResult>());
            var ok = result as OkObjectResult;

            Assert.That(ok?.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(ok?.Value, Is.TypeOf<List<UserResponseDTO>>());

            var body = ok?.Value as List<UserResponseDTO>;
            Assert.That(body, Is.Empty);
        }

        [Test]
        public async Task Get_ShouldReturn500WithProblemDetails_WhenExceptionIsThrown()
        {
            _serviceMock.Setup(s => s.GetAllUsersAsync())
                        .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.Get();

            Assert.That(result, Is.TypeOf<ObjectResult>());
            var obj = result as ObjectResult;

            Assert.That(obj?.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(obj?.Value, Is.TypeOf<ProblemDetails>());

            var problem = obj?.Value as ProblemDetails;
            Assert.That(problem?.Status, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(problem?.Title, Is.EqualTo("Erro interno do servidor"));
            Assert.That(problem?.Detail, Is.EqualTo("Unexpected error"));
        }
    }
}
