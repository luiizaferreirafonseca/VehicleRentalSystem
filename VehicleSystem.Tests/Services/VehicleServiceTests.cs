using Moq;
using NUnit.Framework;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Services;
using VehicleRentalSystem.Resources;
using VehicleRentalSystem.Enums;

namespace VehicleSystem.Tests
{
    [TestFixture]
    public class VehicleServiceTests
    {
        private Mock<IVehicleRepository> _repositoryMock;
        private VehicleService _service;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IVehicleRepository>();
            _service = new VehicleService(_repositoryMock.Object);
        }

        [Test]
        public async Task UpdateVehicleAsync_ShouldUpdateData_WhenRequestIsValid()
        {
            var vehicleId = Guid.NewGuid();
            var existingVehicle = new TbVehicle
            {
                Id = vehicleId,
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2020,
                DailyRate = 200m,
                Status = "available",
                LicensePlate = "ABC-1234"
            };

            var updateDto = new VehicleUpdateDTO
            {
                Year = 2022, 
                DailyRate = 250m, 
                Status = "maintenance" 
            };

            _repositoryMock.Setup(r => r.GetVehicleByIdAsync(vehicleId))
                           .ReturnsAsync(existingVehicle);

            var result = await _service.UpdateVehicleAsync(vehicleId, updateDto);

            Assert.IsNotNull(result);
            Assert.That(result.Year, Is.EqualTo(2022));
            Assert.That(result.DailyRate, Is.EqualTo(250m));
            Assert.That(result.Status, Is.EqualTo("maintenance"));
            _repositoryMock.Verify(r => r.UpdateVehicleAsync(It.IsAny<TbVehicle>()), Times.Once);
        }

        [Test]
        public void UpdateVehicleAsync_ShouldThrow_WhenDailyRateIsZeroOrLess()
        {
            var vehicleId = Guid.NewGuid();
            var updateDto = new VehicleUpdateDTO { Year = 2020, DailyRate = 0, Status = "available" };
            
            _repositoryMock.Setup(r => r.GetVehicleByIdAsync(vehicleId))
                           .ReturnsAsync(new TbVehicle());

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.UpdateVehicleAsync(vehicleId, updateDto));

            Assert.That(ex.Message, Is.EqualTo(Messages.VehicleDailyRateInvalid));
        }

        [Test]
        public void UpdateVehicleAsync_ShouldThrow_WhenYearIsInvalid()
        {
            var vehicleId = Guid.NewGuid();
            var updateDto = new VehicleUpdateDTO { Year = 2050, DailyRate = 100, Status = "available" };
            
            _repositoryMock.Setup(r => r.GetVehicleByIdAsync(vehicleId))
                           .ReturnsAsync(new TbVehicle());

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.UpdateVehicleAsync(vehicleId, updateDto));

            Assert.That(ex.Message, Is.EqualTo(Messages.VehicleYearInvalid));
        }

        [Test]
        public void UpdateVehicleAsync_ShouldThrow_WhenStatusIsInvalid()
        {
            var vehicleId = Guid.NewGuid();
            var updateDto = new VehicleUpdateDTO { Year = 2020, DailyRate = 100, Status = "invalid_status" };
            
            _repositoryMock.Setup(r => r.GetVehicleByIdAsync(vehicleId))
                           .ReturnsAsync(new TbVehicle());

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.UpdateVehicleAsync(vehicleId, updateDto));

            Assert.That(ex.Message, Is.EqualTo("Status inválido. Use: available, rented ou maintenance."));
        }

        [Test]
        public void UpdateVehicleAsync_ShouldThrow_WhenVehicleNotFound()
        {
            var vehicleId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetVehicleByIdAsync(vehicleId))
                           .ReturnsAsync((TbVehicle?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.UpdateVehicleAsync(vehicleId, new VehicleUpdateDTO()));
        }

        [Test]
        public void RemoveVehicleAsync_ShouldThrow_WhenVehicleIdIsEmpty()
        {
            var vehicleId = Guid.Empty;

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.RemoveVehicleAsync(vehicleId));

            Assert.That(ex!.Message, Is.EqualTo(Messages.VehicleIdRequired));

            _repositoryMock.Verify(r =>
                r.GetVehicleByIdAsync(It.IsAny<Guid>()),
                Times.Never);
        }

        [Test]
        public async Task RemoveVehicleAsync_ShouldDeleteVehicle_WhenVehicleIsValid()
        {
            var vehicleId = Guid.NewGuid();

            var vehicle = new TbVehicle
            {
                Id = vehicleId,
                Status = VehicleStatus.available.ToString()
            };

            _repositoryMock.Setup(r => r.GetVehicleByIdAsync(vehicleId))
                           .ReturnsAsync(vehicle);

            _repositoryMock.Setup(r => r.DeleteVehicleAsync(vehicle))
                           .Returns(Task.CompletedTask);

            await _service.RemoveVehicleAsync(vehicleId);

            _repositoryMock.Verify(r => r.GetVehicleByIdAsync(vehicleId), Times.Once);
            _repositoryMock.Verify(r => r.DeleteVehicleAsync(vehicle), Times.Once);
        }

        #region GetAvailableVehiclesAsync

        [Test]
        public async Task GetAvailableVehiclesAsync_ComVeiculosDisponiveis_RetornaListaEMensagemNula()
        {
            var vehicles = new List<TbVehicle>
            {
                new() { Id = Guid.NewGuid(), Brand = "Honda",  Model = "Civic",   Year = 2022, DailyRate = 150m, Status = "available", LicensePlate = "AAA-1111" },
                new() { Id = Guid.NewGuid(), Brand = "Toyota", Model = "Corolla", Year = 2021, DailyRate = 200m, Status = "available", LicensePlate = "BBB-2222" }
            };
            _repositoryMock.Setup(r => r.SearchVehiclesAsync("available", 1)).ReturnsAsync(vehicles);

            var result = await _service.GetAvailableVehiclesAsync(1);

            Assert.Multiple(() =>
            {
                Assert.That(result.Vehicles, Has.Count.EqualTo(2));
                Assert.That(result.Message, Is.Null);
            });
        }

        [Test]
        public async Task GetAvailableVehiclesAsync_SemVeiculosDisponiveis_RetornaListaVaziaEMensagem()
        {
            _repositoryMock.Setup(r => r.SearchVehiclesAsync("available", 1)).ReturnsAsync(new List<TbVehicle>());

            var result = await _service.GetAvailableVehiclesAsync(1);

            Assert.Multiple(() =>
            {
                Assert.That(result.Vehicles, Is.Empty);
                Assert.That(result.Message, Is.EqualTo("Não há veículos disponíveis para locação."));
            });
        }

        [Test]
        public async Task GetAvailableVehiclesAsync_ChamaRepositorioComStatusAvailable()
        {
            _repositoryMock.Setup(r => r.SearchVehiclesAsync("available", 1)).ReturnsAsync(new List<TbVehicle>());

            await _service.GetAvailableVehiclesAsync(1);

            _repositoryMock.Verify(r => r.SearchVehiclesAsync("available", 1), Times.Once);
        }

        [Test]
        public async Task GetAvailableVehiclesAsync_PaginaCustomizada_PassaPaginaCorretaAoRepositorio()
        {
            _repositoryMock.Setup(r => r.SearchVehiclesAsync("available", 3)).ReturnsAsync(new List<TbVehicle>());

            await _service.GetAvailableVehiclesAsync(3);

            _repositoryMock.Verify(r => r.SearchVehiclesAsync("available", 3), Times.Once);
        }

        [Test]
        public async Task GetAvailableVehiclesAsync_MapeiaPropriedadesDosVeiculosCorretamente()
        {
            var vehicle = new TbVehicle
            {
                Id = Guid.NewGuid(), Brand = "Ford", Model = "Ka",
                Year = 2020, DailyRate = 100m, Status = "available", LicensePlate = "XYZ-9999"
            };
            _repositoryMock.Setup(r => r.SearchVehiclesAsync("available", 1)).ReturnsAsync(new List<TbVehicle> { vehicle });

            var result = await _service.GetAvailableVehiclesAsync(1);
            var dto = result.Vehicles[0];

            Assert.Multiple(() =>
            {
                Assert.That(dto.Id, Is.EqualTo(vehicle.Id));
                Assert.That(dto.Brand, Is.EqualTo("Ford"));
                Assert.That(dto.Model, Is.EqualTo("Ka"));
                Assert.That(dto.Status, Is.EqualTo("available"));
                Assert.That(dto.LicensePlate, Is.EqualTo("XYZ-9999"));
            });
        }

        #endregion

        [Test]
        public async Task SearchVehiclesAsync_ShouldReturnVehicleDTOList_WhenVehiclesExist()
        {
            var status = "available";
            var page = 1;

            var vehiclesFromDb = new List<TbVehicle>
        {
            new TbVehicle
            {
                Id = Guid.NewGuid(),
                Brand = "Chevrolet",
                Model = "Onix",
                Year = 2022,
                DailyRate = 150m,
                Status = "available",
                LicensePlate = "ABC1D23"
            },
            new TbVehicle
            {
                Id = Guid.NewGuid(),
                Brand = "Fiat",
                Model = "Argo",
                Year = 2021,
                DailyRate = 140m,
                Status = "available",
                LicensePlate = "XYZ9K88"
            }
        };

            _repositoryMock.Setup(r => r.SearchVehiclesAsync(status, page))
                           .ReturnsAsync(vehiclesFromDb);

            var result = await _service.SearchVehiclesAsync(status, page);

            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(2));

            Assert.That(result[0].Id, Is.EqualTo(vehiclesFromDb[0].Id));
            Assert.That(result[0].Brand, Is.EqualTo("Chevrolet"));
            Assert.That(result[0].Model, Is.EqualTo("Onix"));
            Assert.That(result[0].Year, Is.EqualTo(2022));
            Assert.That(result[0].DailyRate, Is.EqualTo(150m));
            Assert.That(result[0].Status, Is.EqualTo("available"));
            Assert.That(result[0].LicensePlate, Is.EqualTo("ABC1D23"));

            Assert.That(result[1].Id, Is.EqualTo(vehiclesFromDb[1].Id));
            Assert.That(result[1].Brand, Is.EqualTo("Fiat"));
            Assert.That(result[1].Model, Is.EqualTo("Argo"));
            Assert.That(result[1].Year, Is.EqualTo(2021));
            Assert.That(result[1].DailyRate, Is.EqualTo(140m));
            Assert.That(result[1].Status, Is.EqualTo("available"));
            Assert.That(result[1].LicensePlate, Is.EqualTo("XYZ9K88"));
        }

        [Test]
        public async Task SearchVehiclesAsync_ShouldReturnEmptyList_WhenNoVehiclesExist()
        {
            var status = "available";
            var page = 1;

            _repositoryMock.Setup(r => r.SearchVehiclesAsync(status, page))
                           .ReturnsAsync(new List<TbVehicle>());

            var result = await _service.SearchVehiclesAsync(status, page);

            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task CreateVehicleAsync_ShouldCreateVehicle_WhenRequestIsValid()
        {
            var dto = new VehicleCreateDTO
            {
                Brand = "Chevrolet",
                Model = "Onix",
                Year = 2022,
                DailyRate = 150m,
                LicensePlate = " ABC1234 "
            };

            _repositoryMock.Setup(r => r.ExistsByLicensePlateAsync("ABC1234"))
                           .ReturnsAsync(false);

            _repositoryMock.Setup(r => r.CreateVehicleAsync(It.IsAny<TbVehicle>()))
                           .ReturnsAsync((TbVehicle v) => v);

            var result = await _service.CreateVehicleAsync(dto);

            Assert.IsNotNull(result);
            Assert.That(result.Brand, Is.EqualTo("Chevrolet"));
            Assert.That(result.Model, Is.EqualTo("Onix"));
            Assert.That(result.Year, Is.EqualTo(2022));
            Assert.That(result.DailyRate, Is.EqualTo(150m));
            Assert.That(result.LicensePlate, Is.EqualTo("ABC1234"));
            Assert.That(result.Status, Is.EqualTo(VehicleStatus.available.ToString()));

            _repositoryMock.Verify(r => r.ExistsByLicensePlateAsync("ABC1234"), Times.Once);
            _repositoryMock.Verify(r => r.CreateVehicleAsync(It.IsAny<TbVehicle>()), Times.Once);
        }

        [Test]
        public void CreateVehicleAsync_ShouldThrow_WhenBrandIsEmpty()
        {
            var dto = new VehicleCreateDTO
            {
                Brand = " ",
                Model = "Onix",
                Year = 2022,
                DailyRate = 150m,
                LicensePlate = "ABC1234"
            };

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.CreateVehicleAsync(dto));

            Assert.That(ex!.Message, Is.EqualTo(Messages.VehicleBrandRequired));

            _repositoryMock.Verify(r => r.ExistsByLicensePlateAsync(It.IsAny<string>()), Times.Never);
            _repositoryMock.Verify(r => r.CreateVehicleAsync(It.IsAny<TbVehicle>()), Times.Never);
        }

        [Test]
        public void CreateVehicleAsync_ShouldThrow_WhenModelIsEmpty()
        {
            var dto = new VehicleCreateDTO
            {
                Brand = "Chevrolet",
                Model = " ",
                Year = 2022,
                DailyRate = 150m,
                LicensePlate = "ABC1234"
            };

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.CreateVehicleAsync(dto));

            Assert.That(ex!.Message, Is.EqualTo(Messages.VehicleModelRequired));

            _repositoryMock.Verify(r => r.ExistsByLicensePlateAsync(It.IsAny<string>()), Times.Never);
            _repositoryMock.Verify(r => r.CreateVehicleAsync(It.IsAny<TbVehicle>()), Times.Never);
        }

        [Test]
        public void CreateVehicleAsync_ShouldThrow_WhenYearIsInvalid()
        {
            var dto = new VehicleCreateDTO
            {
                Brand = "Chevrolet",
                Model = "Onix",
                Year = 0,
                DailyRate = 150m,
                LicensePlate = "ABC1234"
            };

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.CreateVehicleAsync(dto));

            Assert.That(ex!.Message, Is.EqualTo(Messages.VehicleYearInvalid));

            _repositoryMock.Verify(r => r.ExistsByLicensePlateAsync(It.IsAny<string>()), Times.Never);
            _repositoryMock.Verify(r => r.CreateVehicleAsync(It.IsAny<TbVehicle>()), Times.Never);
        }

        [Test]
        public void CreateVehicleAsync_ShouldThrow_WhenDailyRateIsInvalid()
        {
            var dto = new VehicleCreateDTO
            {
                Brand = "Chevrolet",
                Model = "Onix",
                Year = 2022,
                DailyRate = 0m,
                LicensePlate = "ABC1234"
            };

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.CreateVehicleAsync(dto));

            Assert.That(ex!.Message, Is.EqualTo(Messages.VehicleDailyRateInvalid));

            _repositoryMock.Verify(r => r.ExistsByLicensePlateAsync(It.IsAny<string>()), Times.Never);
            _repositoryMock.Verify(r => r.CreateVehicleAsync(It.IsAny<TbVehicle>()), Times.Never);
        }
    }
}