using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            var userDto = new UserCreateDTO { Name = "Duplicado", Email = "existe@teste.com" };

            _repositoryMock
                .Setup(x => x.ExistsByEmailAsync(userDto.Email))
                .ReturnsAsync(true);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _service.CreateUserAsync(userDto));

            Assert.That(ex.Message, Is.EqualTo("This email is already registered in the system."));
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
            var dto = new UserCreateDTO { Name = "Ale", Email = "" }; 

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.CreateUserAsync(dto));

            Assert.That(ex.Message, Is.EqualTo(Messages.UserEmailMissing));
        }

        [Test]
        public void CreateUserAsync_ShouldThrow_WhenNameIsMissing()
        {
            var dto = new UserCreateDTO { Name = "   ", Email = "ok@email.com" };

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.CreateUserAsync(dto));

            Assert.That(ex!.Message, Is.EqualTo(Messages.UserNameMissing));

            // valida que nem chega a consultar repo
            _repositoryMock.Verify(r => r.ExistsByEmailAsync(It.IsAny<string>()), Times.Never);
            _repositoryMock.Verify(r => r.CreateUserAsync(It.IsAny<TbUser>()), Times.Never);
        }

        [Test]
        public void CreateUserAsync_ShouldThrow_WhenEmailIsWhitespace()
        {
            var dto = new UserCreateDTO { Name = "Ale", Email = "   " };

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.CreateUserAsync(dto));

            Assert.That(ex!.Message, Is.EqualTo(Messages.UserEmailMissing));

            _repositoryMock.Verify(r => r.ExistsByEmailAsync(It.IsAny<string>()), Times.Never);
            _repositoryMock.Verify(r => r.CreateUserAsync(It.IsAny<TbUser>()), Times.Never);
        }

        [Test]
        public async Task CreateUserAsync_ShouldTrimName_AndTrimLowerEmail_BeforeSaving()
        {
            var dto = new UserCreateDTO
            {
                Name = "  Ale Teste  ",
                Email = "  ALE@TESTE.COM  "
            };

            _repositoryMock
                .Setup(r => r.ExistsByEmailAsync(dto.Email))
                .ReturnsAsync(false);

            TbUser? captured = null;

            _repositoryMock
                .Setup(r => r.CreateUserAsync(It.IsAny<TbUser>()))
                .Callback<TbUser>(u => captured = u)
                .ReturnsAsync((TbUser u) => u); // devolve o mesmo que foi salvo

            var result = await _service.CreateUserAsync(dto);

            Assert.That(captured, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(captured!.Name, Is.EqualTo("Ale Teste"));
                Assert.That(captured.Email, Is.EqualTo("ale@teste.com"));
                Assert.That(captured.Active, Is.True);
                Assert.That(captured.Id, Is.Not.EqualTo(Guid.Empty));

                // resposta também deve refletir o que veio do repositório
                Assert.That(result.Name, Is.EqualTo("Ale Teste"));
                Assert.That(result.Email, Is.EqualTo("ale@teste.com"));
                Assert.That(result.Active, Is.True);
            });

            _repositoryMock.Verify(r => r.ExistsByEmailAsync(dto.Email), Times.Once);
            _repositoryMock.Verify(r => r.CreateUserAsync(It.IsAny<TbUser>()), Times.Once);
        }

        [Test]
        public async Task CreateUserAsync_ShouldReturnEmptyRentalsList()
        {
            var dto = new UserCreateDTO { Name = "Lu", Email = "lu@teste.com" };

            _repositoryMock
                .Setup(r => r.ExistsByEmailAsync(dto.Email))
                .ReturnsAsync(false);

            _repositoryMock
                .Setup(r => r.CreateUserAsync(It.IsAny<TbUser>()))
                .ReturnsAsync((TbUser u) => u);

            var result = await _service.CreateUserAsync(dto);

            Assert.That(result.Rentals, Is.Not.Null);
            Assert.That(result.Rentals.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetAllUsersAsync_ShouldThrow_WhenUserEmailIsMissing()
        {
            var usersFromDb = new List<TbUser>
            {
                new TbUser
                {
                    Id = Guid.NewGuid(),
                    Name = "ok",
                    Email = "   ",
                    Active = true,
                    TbRentals = new List<TbRental>()
                }
            };

            _repositoryMock.Setup(r => r.GetAllUsersAsync())
                           .ReturnsAsync(usersFromDb);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetAllUsersAsync());

            Assert.That(ex!.Message, Is.EqualTo(Messages.UserEmailMissing));
        }

        [Test]
        public async Task GetAllUsersAsync_ShouldMapEmptyRentals_WhenUserHasNoRentals()
        {
            var userId = Guid.NewGuid();

            var usersFromDb = new List<TbUser>
            {
                new TbUser
                {
                    Id = userId,
                    Name = "Rafa",
                    Email = "rafa@email.com",
                    Active = false,
                    TbRentals = new List<TbRental>() // vazio
                }
            };

            _repositoryMock.Setup(r => r.GetAllUsersAsync())
                           .ReturnsAsync(usersFromDb);

            var result = await _service.GetAllUsersAsync();

            Assert.That(result.Count, Is.EqualTo(1));

            var dto = result[0];
            Assert.Multiple(() =>
            {
                Assert.That(dto.Id, Is.EqualTo(userId));
                Assert.That(dto.Name, Is.EqualTo("Rafa"));
                Assert.That(dto.Email, Is.EqualTo("rafa@email.com"));
                Assert.That(dto.Active, Is.False);

                Assert.That(dto.Rentals, Is.Not.Null);
                Assert.That(dto.Rentals.Count, Is.EqualTo(0));
            });
        }
    }
}

