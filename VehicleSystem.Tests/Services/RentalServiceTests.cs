using Moq;
using NUnit.Framework;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Services;
using VehicleRentalSystem.Enums;
using VehicleRentalSystem.Resources;
using VehicleRentalSystem.Enums.VehicleRentalSystem.Enums;

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

        [Test]
        public void ReturnRentalAsync_ShouldThrow_WhenRentalNotFound()
        {
            var rentalId = Guid.NewGuid();

            _repositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId))
                           .ReturnsAsync((TbRental?)null);

            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.ReturnRentalAsync(rentalId));

            Assert.That(ex.Message, Is.EqualTo(Messages.RentalNotFound));
        }

        [Test]
        public async Task ReturnRentalAsync_WithPenalty_WhenReturnedAfterExpectedDate()
        {
            var rentalId = Guid.NewGuid();

            var rental = new TbRental
            {
                Id = rentalId,
                StartDate = DateTime.UtcNow.AddDays(-10),
                ExpectedEndDate = DateTime.UtcNow.AddDays(-3), 
                ActualEndDate = null,
                TotalAmount = 1000m,
                DailyRate = 100m,
                PenaltyFee = 0m,
                Status = RentalStatus.active.ToString(),
                VehicleId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Vehicle = new TbVehicle { Status = VehicleStatus.rented.ToString() }
            };

            _repositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId))
                           .ReturnsAsync(rental);

            _repositoryMock.Setup(r => r.SaveChangesAsync())
                           .Returns(Task.CompletedTask);

            var result = await _service.ReturnRentalAsync(rentalId);

            Assert.That(result.PenaltyFee, Is.GreaterThan(0m));
            Assert.That(result.Status, Is.EqualTo(RentalStatus.completed.ToString()));
            Assert.That(rental.Vehicle.Status, Is.EqualTo(VehicleStatus.available.ToString()));
        }

        [Test]
        public void GetRentalById_ShouldReturnRentalDTO_WhenIdExists()
        {
            var rentalId = Guid.NewGuid();
            var rentalFromDb = new TbRental
            {
                Id = rentalId,
                StartDate = DateTime.UtcNow,
                User = new TbUser { Name = "Ale Teste" },
                Vehicle = new TbVehicle { Model = "Onix" },
                Status = "active"
            };

            _repositoryMock.Setup(r => r.SelectRentalById(rentalId))
                           .Returns(rentalFromDb);

            var result = _service.GetRentalById(rentalId);

            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(rentalId));
            Assert.That(result.UserName, Is.EqualTo("Ale Teste"));
            Assert.That(result.VehicleModel, Is.EqualTo("Onix"));
        }

        [Test]
        public void GetRentalById_ShouldReturnNull_WhenIdDoesNotExist()
        {
            var rentalId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.SelectRentalById(rentalId))
                           .Returns((TbRental?)null);

            var result = _service.GetRentalById(rentalId);

            Assert.IsNull(result);
        }

        [Test]
        [TestCase("active")]
        [TestCase("completed")]
        [TestCase("canceled")]
        public async Task SearchRentalsByUserAsync_ShouldReturnList_ForEveryStatus(string status)
        {
            var userId = Guid.NewGuid();
            var rentalsFromDb = new List<TbRental>
            {
                new TbRental 
                { 
                    Id = Guid.NewGuid(), 
                    UserId = userId, 
                    Status = status,
                    User = new TbUser { Name = "Ale Cliente" },
                    Vehicle = new TbVehicle { Model = "Civic" }
                }
            };

            _repositoryMock.Setup(r => r.SearchRentalsByUserAsync(userId, status, 1))
                           .ReturnsAsync(rentalsFromDb);

            var result = await _service.SearchRentalsByUserAsync(userId, status, 1);

            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Status, Is.EqualTo(status));
            Assert.That(result[0].UserName, Is.EqualTo("Ale Cliente"));
            Assert.That(result[0].VehicleModel, Is.EqualTo("Civic"));
        }

        [Test]
        public void SearchRentalsByUserAsync_ShouldThrow_WhenPageIsLessThanOne()
        {
            var userId = Guid.NewGuid();

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.SearchRentalsByUserAsync(userId, "active", 0));

            Assert.That(ex.Message, Is.EqualTo(Messages.PageInvalid));
        }

        // --- TESTES DE ATUALIZAÇÃO DE LOCAÇÃO ---

        [Test]
        public async Task UpdateRentalDatesAsync_ShouldRecalculateTotalAmount_WhenDatesAreUpdated()
        {
            var rentalId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-5);
            var oldExpectedDate = DateTime.UtcNow.AddDays(2); 
            var newExpectedDate = DateTime.UtcNow.AddDays(5); 
            var dailyRate = 100m;

            var existingRental = new TbRental
            {
                Id = rentalId,
                StartDate = startDate,
                ExpectedEndDate = oldExpectedDate,
                DailyRate = dailyRate,
                Status = RentalStatus.active.ToString(), 
                TotalAmount = 700m
            };

            _repositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId))
                           .ReturnsAsync(existingRental);

            var updateDto = new UpdateRentalDTO { NewExpectedEndDate = newExpectedDate };

            var result = await _service.UpdateRentalDatesAsync(rentalId, updateDto);

            Assert.IsNotNull(result);
            Assert.That(result.ExpectedEndDate, Is.EqualTo(newExpectedDate));
            Assert.That(result.TotalAmount, Is.EqualTo(1000m)); 
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<TbRental>()), Times.Once);
        }

        [Test]
        public void UpdateRentalDatesAsync_ShouldThrow_WhenRentalIsNotActive()
        {
            var rentalId = Guid.NewGuid();
            var existingRental = new TbRental
            {
                Id = rentalId,
                Status = RentalStatus.completed.ToString()
            };

            _repositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId))
                           .ReturnsAsync(existingRental);

            var updateDto = new UpdateRentalDTO { NewExpectedEndDate = DateTime.UtcNow.AddDays(1) };

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.UpdateRentalDatesAsync(rentalId, updateDto)); 
            
            Assert.That(ex.Message, Does.StartWith("Só é permitido atualizar locações que estejam 'active'"));
        }
    }
}