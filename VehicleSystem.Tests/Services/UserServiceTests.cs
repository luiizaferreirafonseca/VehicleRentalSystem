using Moq;
using NUnit.Framework;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Resources;
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

        [Test]
        public void GetAllUsersAsync_ShouldThrow_WhenUserNameIsMissing()
        {
            var usersFromDb = new List<TbUser>
            {
                new TbUser
                {
                    Id = Guid.NewGuid(),
                    Name = "", 
                    Email = "teste@email.com",
                    Active = true,
                    TbRentals = new List<TbRental>()
                }
            };

            _repositoryMock.Setup(r => r.GetAllUsersAsync())
                           .ReturnsAsync(usersFromDb);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetAllUsersAsync());

            Assert.That(ex!.Message, Is.EqualTo(Messages.UserNameMissing));
        }

        [Test]
        public async Task GetAllUsersAsync_ShouldReturnMappedUsers_WithRentals()
        {
            var userId = Guid.NewGuid();
            var rentalId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();

            var usersFromDb = new List<TbUser>
            {
                new TbUser
                {
                    Id = userId,
                    Name = "Luiza",
                    Email = "luiza@email.com",
                    Active = true,
                    TbRentals = new List<TbRental>
                    {
                        new TbRental
                        {
                            Id = rentalId,
                            VehicleId = vehicleId
                        }
                    }
                }
            };

            _repositoryMock.Setup(r => r.GetAllUsersAsync())
                           .ReturnsAsync(usersFromDb);

            var result = await _service.GetAllUsersAsync();

            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(1));

            var dto = result[0];
            Assert.That(dto.Id, Is.EqualTo(userId));
            Assert.That(dto.Name, Is.EqualTo("Luiza"));
            Assert.That(dto.Email, Is.EqualTo("luiza@email.com"));
            Assert.That(dto.Active, Is.True);

            Assert.That(dto.Rentals, Is.Not.Null);
            Assert.That(dto.Rentals.Count, Is.EqualTo(1));
            Assert.That(dto.Rentals[0].RentalId, Is.EqualTo(rentalId));
            Assert.That(dto.Rentals[0].VehicleId, Is.EqualTo(vehicleId));
        }

        [Test]
        public void CreateUserAsync_ShouldThrow_WhenEmailIsMissing()
        {
            // Arrange
            var dto = new UserCreateDTO { Name = "Ale", Email = "" }; 

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.CreateUserAsync(dto));

            Assert.That(ex.Message, Is.EqualTo(Messages.UserEmailMissing));
        }

        [Test]
        public void GetAllUsersAsync_ShouldThrow_WhenUserEmailIsMissing()
        {
            // Arrange
            var usersFromDb = new List<TbUser>
            {
                new TbUser { Id = Guid.NewGuid(), Name = "Luiza", Email = null } 
            };

            _repositoryMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(usersFromDb);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetAllUsersAsync());

            Assert.That(ex.Message, Is.EqualTo(Messages.UserEmailMissing));
        }
    }
}
