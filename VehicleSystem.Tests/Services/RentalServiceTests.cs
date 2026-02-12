using Moq;
using NUnit.Framework;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Services;
using VehicleRentalSystem.Enums;

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
    }
}