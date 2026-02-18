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
            // Arrange
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

            // Act
            var result = await _service.UpdateVehicleAsync(vehicleId, updateDto);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Year, Is.EqualTo(2022));
            Assert.That(result.DailyRate, Is.EqualTo(250m));
            Assert.That(result.Status, Is.EqualTo("maintenance"));
            _repositoryMock.Verify(r => r.UpdateVehicleAsync(It.IsAny<TbVehicle>()), Times.Once);
        }

        [Test]
        public void UpdateVehicleAsync_ShouldThrow_WhenDailyRateIsZeroOrLess()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var updateDto = new VehicleUpdateDTO { Year = 2020, DailyRate = 0, Status = "available" };
            
            _repositoryMock.Setup(r => r.GetVehicleByIdAsync(vehicleId))
                           .ReturnsAsync(new TbVehicle());

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.UpdateVehicleAsync(vehicleId, updateDto));

            Assert.That(ex.Message, Is.EqualTo(Messages.VehicleDailyRateInvalid));
        }

        [Test]
        public void UpdateVehicleAsync_ShouldThrow_WhenYearIsInvalid()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var updateDto = new VehicleUpdateDTO { Year = 2050, DailyRate = 100, Status = "available" };
            
            _repositoryMock.Setup(r => r.GetVehicleByIdAsync(vehicleId))
                           .ReturnsAsync(new TbVehicle());

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.UpdateVehicleAsync(vehicleId, updateDto));

            Assert.That(ex.Message, Is.EqualTo(Messages.VehicleYearInvalid));
        }

        [Test]
        public void UpdateVehicleAsync_ShouldThrow_WhenStatusIsInvalid()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var updateDto = new VehicleUpdateDTO { Year = 2020, DailyRate = 100, Status = "invalid_status" };
            
            _repositoryMock.Setup(r => r.GetVehicleByIdAsync(vehicleId))
                           .ReturnsAsync(new TbVehicle());

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.UpdateVehicleAsync(vehicleId, updateDto));

            Assert.That(ex.Message, Is.EqualTo("Status inválido. Use: available, rented ou maintenance."));
        }

        [Test]
        public void UpdateVehicleAsync_ShouldThrow_WhenVehicleNotFound()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.GetVehicleByIdAsync(vehicleId))
                           .ReturnsAsync((TbVehicle?)null);

            // Act & Assert
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
    }
}