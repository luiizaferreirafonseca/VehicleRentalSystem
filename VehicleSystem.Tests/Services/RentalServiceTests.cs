using Moq;
using NUnit.Framework;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Services;
using VehicleRentalSystem.Enums;
using VehicleRentalSystem.Resources;

namespace VehicleSystem.Tests
{
    [TestFixture]
    public class RentalServiceTests
    {
        private Mock<IRentalRepository> _repositoryMock;
        private RentalService _service;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IRentalRepository>();
            _service = new RentalService(_repositoryMock.Object);
        }

        [Test]
        public async Task UpdateRentalDatesAsync_ValidData_ReturnsUpdatedRental()
        {
            var rentalId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-2);
            var oldExpectedDate = DateTime.UtcNow.AddDays(2);
            var newExpectedDate = DateTime.UtcNow.AddDays(5); 

            var existingRental = new TbRental
            {
                Id = rentalId,
                StartDate = startDate,
                ExpectedEndDate = oldExpectedDate,
                DailyRate = 100,
                Status = "active"
            };

            _repositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId))
                           .ReturnsAsync(existingRental);

            var updateDto = new UpdateRentalDTO { NewExpectedEndDate = newExpectedDate };

            var result = await _service.UpdateRentalDatesAsync(rentalId, updateDto);

            Assert.IsNotNull(result);
            Assert.That(result.ExpectedEndDate, Is.EqualTo(newExpectedDate));
            Assert.That(result.TotalAmount, Is.EqualTo(700)); 
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TbRental>()), Times.Once);
        }

        [Test]
        public void UpdateRentalDatesAsync_InvalidDate_ThrowsException()
        {
            var rentalId = Guid.NewGuid();
            var existingRental = new TbRental
            {
                Id = rentalId,
                StartDate = DateTime.UtcNow.AddDays(5),
                Status = "active"
            };

            _repositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId)).ReturnsAsync(existingRental);

            var updateDto = new UpdateRentalDTO { NewExpectedEndDate = DateTime.UtcNow };

      
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await _service.UpdateRentalDatesAsync(rentalId, updateDto));
            
            Assert.That(ex.Message, Is.EqualTo("A nova data de devolução deve ser posterior à data de início."));
        }

        //Create Rental Service Tests - luiza 
        [Test]
        public async Task CreateRentalAsync_ShouldReturnDTO_WhenDataIsValid()
        {
            var userId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();

            var dto = new RentalCreateDTO
            {
                UserId = userId,
                VehicleId = vehicleId,
                StartDate = new DateTime(2026, 02, 02),
                ExpectedEndDate = new DateTime(2026, 02, 05)
            };

            var user = new TbUser { Id = userId, Name = "Thamires" };
            var vehicle = new TbVehicle { Id = vehicleId, Model = "Onix", DailyRate = 150m };

            _repositoryMock.Setup(x => x.GetUserById(userId)).ReturnsAsync(user);
            _repositoryMock.Setup(x => x.GetVehicleById(vehicleId)).ReturnsAsync(vehicle);
            _repositoryMock.Setup(x => x.CreateRentalAsync(It.IsAny<TbRental>()))
                           .ReturnsAsync((TbRental r) => r);
            _repositoryMock.Setup(x => x.UpdateVehicleStatusAsync(vehicleId, It.IsAny<string>()))
                           .ReturnsAsync(true);

            var result = await _service.CreateRentalAsync(dto);

            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.VehicleId, Is.EqualTo(vehicleId));
            Assert.That(result.UserName, Is.EqualTo(user.Name));
            Assert.That(result.VehicleModel, Is.EqualTo(vehicle.Model));
            Assert.That(result.DailyRate, Is.EqualTo(vehicle.DailyRate));
            Assert.That(result.TotalAmount, Is.EqualTo(450m));
            Assert.That(result.PenaltyFee, Is.EqualTo(0m));
        }

        [Test]
        public void CreateRentalAsync_ShouldThrow_WhenUserIdIsEmpty()
        {
            var dto = new RentalCreateDTO
            {
                UserId = Guid.Empty,
                VehicleId = Guid.NewGuid(),
                StartDate = new DateTime(2026, 02, 02),
                ExpectedEndDate = new DateTime(2026, 02, 05)
            };

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.CreateRentalAsync(dto));
        }
    }
}