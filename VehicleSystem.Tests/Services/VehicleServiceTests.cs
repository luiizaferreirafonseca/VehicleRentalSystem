using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Enums;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Resources;
using VehicleRentalSystem.Services;

namespace VehicleSystem.Tests.Services
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
        public async Task CreateVehicleAsync_ShouldReturnDTO_WhenDataIsValid()
        {
            var dto = new VehicleCreateDTO
            {
                Brand = "  Chevrolet  ",
                Model = "  Onix  ",
                Year = 2022,
                DailyRate = 150m,
                LicensePlate = "  ABC1234  "
            };

            _repositoryMock.Setup(r => r.ExistsByLicensePlateAsync("ABC1234"))
                           .ReturnsAsync(false);

            _repositoryMock.Setup(r => r.CreateVehicleAsync(It.IsAny<TbVehicle>()))
                           .ReturnsAsync((TbVehicle v) => v);

            var result = await _service.CreateVehicleAsync(dto);

            Assert.That(result.Brand, Is.EqualTo("Chevrolet"));
            Assert.That(result.Model, Is.EqualTo("Onix"));
            Assert.That(result.Year, Is.EqualTo(2022));
            Assert.That(result.DailyRate, Is.EqualTo(150m));
            Assert.That(result.LicensePlate, Is.EqualTo("ABC1234"));
            Assert.That(result.Status, Is.EqualTo("available"));
        }

        [Test]
        public void CreateVehicleAsync_ShouldThrow_WhenLicensePlateAlreadyExists()
        {
            var dto = new VehicleCreateDTO
            {
                Brand = "Fiat",
                Model = "Argo",
                Year = 2021,
                DailyRate = 120m,
                LicensePlate = "ABC1234"
            };

            _repositoryMock.Setup(r => r.ExistsByLicensePlateAsync("ABC1234"))
                           .ReturnsAsync(true); 

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.CreateVehicleAsync(dto));

            Assert.That(ex.Message, Is.EqualTo(Messages.VehicleLicensePlateAlreadyExists));
        }

        [Test]
        public void RemoveVehicleAsync_WhenVehicleIsRented_ShouldThrowException()
        {
            var vehicleId = Guid.NewGuid();

            var vehicle = new TbVehicle
            {
                Id = vehicleId,
                Status = VehicleStatus.rented.ToString()
            };

            _repositoryMock.Setup(r => r.GetVehicleByIdAsync(vehicleId))
                           .ReturnsAsync(vehicle);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.RemoveVehicleAsync(vehicleId)
            );

            Assert.That(ex.Message, Is.EqualTo(Messages.VehicleCannotBeDeletedWhenRented));
        }

    }
}
