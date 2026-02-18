using Moq;
using NUnit.Framework;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Services;

namespace VehicleSystem.Tests.Services
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<IUserRepository> _repositoryMock;
        private UserService _service;

        [SetUp]
        public void Setup()
        {
            _repositoryMock = new Mock<IUserRepository>();
            _service = new UserService(_repositoryMock.Object);
        }

        [Test]
        public async Task CreateUserAsync_QuandoEmailForNovo_DeveRetornarUsuario()
        {
            var userDto = new UserCreateDTO { Name = "Ale Teste", Email = "ale@teste.com" };
            var userModel = new TbUser { Name = "Ale Teste", Email = "ale@teste.com" };

            _repositoryMock
                .Setup(x => x.ExistsByEmailAsync(userDto.Email))
                .ReturnsAsync(false);

            _repositoryMock
                .Setup(x => x.CreateUserAsync(It.IsAny<TbUser>()))
                .ReturnsAsync(userModel);

            var resultado = await _service.CreateUserAsync(userDto);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Email, Is.EqualTo(userDto.Email));
            _repositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<TbUser>()), Times.Once);
        }

        [Test]
        public void CreateUserAsync_QuandoEmailJaExistir_DeveLancarExcecao()
        {
            // Arrange
            var userDto = new UserCreateDTO { Name = "Duplicado", Email = "existe@teste.com" };

            _repositoryMock
                .Setup(x => x.ExistsByEmailAsync(userDto.Email))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _service.CreateUserAsync(userDto));

            Assert.That(ex.Message, Is.EqualTo("Este e-mail já está cadastrado no sistema."));
            _repositoryMock.Verify(x => x.CreateUserAsync(It.IsAny<TbUser>()), Times.Never);
        }
    }
}