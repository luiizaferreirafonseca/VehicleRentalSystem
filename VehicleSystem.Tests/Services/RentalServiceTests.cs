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

        /// <summary>
        /// Validates that updating rental dates with valid data recalculates
        /// the total amount correctly and persists the changes via the repository.
        /// </summary>
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

        /// <summary>
        /// Ensures that setting a new expected end date earlier than the start date
        /// throws an InvalidOperationException with the correct message.
        /// </summary>
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
            
            Assert.That(ex.Message, Is.EqualTo("The new return date must be later than the start date."));
        }

        /// <summary>
        /// Main rental creation flow: verifies that a valid DTO results in a correctly
        /// mapped response with user name, vehicle model, daily rate, and calculated total amount.
        /// </summary>
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

        /// <summary>
        /// Validates that an empty UserId is rejected before reaching the repository layer,
        /// throwing an InvalidOperationException.
        /// </summary>
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

        /// <summary>
        /// Ensures that attempting to return a non-existent rental throws
        /// a KeyNotFoundException with the correct message.
        /// </summary>
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

        /// <summary>
        /// Verifies that a late return applies a positive penalty fee, marks the rental
        /// as completed, and releases the vehicle back to available status.
        /// </summary>
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

        /// <summary>
        /// Tests successful retrieval of a rental by ID, verifying that user name
        /// and vehicle model are correctly mapped in the response DTO.
        /// </summary>
        [Test]
        public async Task GetRentalByIdAsync_ShouldReturnRentalDTO_WhenIdExists()
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

            _repositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId))
                           .ReturnsAsync(rentalFromDb);

            var result = await _service.GetRentalByIdAsync(rentalId);

            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(rentalId));
            Assert.That(result.UserName, Is.EqualTo("Ale Teste"));
            Assert.That(result.VehicleModel, Is.EqualTo("Onix"));
        }


        /// <summary>
        /// Validates that querying a non-existent rental ID throws a KeyNotFoundException.
        /// </summary>
        [Test]
        public void GetRentalByIdAsync_ShouldThrowKeyNotFoundException_WhenIdDoesNotExist()
        {
            var rentalId = Guid.NewGuid();

            _repositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId))
                           .ReturnsAsync((TbRental?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.GetRentalByIdAsync(rentalId));
        }

        /// <summary>
        /// Parameterized test that ensures rental search returns correctly mapped results
        /// for each valid status value: active, completed, and canceled.
        /// </summary>
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

        /// <summary>
        /// Ensures that a page number less than 1 is rejected with an
        /// InvalidOperationException carrying the correct validation message.
        /// </summary>
        [Test]
        public void SearchRentalsByUserAsync_ShouldThrow_WhenPageIsLessThanOne()
        {
            var userId = Guid.NewGuid();

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.SearchRentalsByUserAsync(userId, "active", 0));

            Assert.That(ex.Message, Is.EqualTo(Messages.PageInvalid));
        }

        /// <summary>
        /// Validates that the total amount is correctly recalculated based on the
        /// daily rate and the new date range after a successful date update.
        /// </summary>
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

        /// <summary>
        /// Ensures that updating dates on a non-active rental throws an
        /// InvalidOperationException describing the current invalid status.
        /// </summary>
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
            
            Assert.That(ex.Message, Does.StartWith("Only rentals with status 'active' can be updated. Current status: completed"));
        }

        /// <summary>
        /// Validates that canceling an active rental sets the status to canceled,
        /// releases the vehicle to available, and saves the changes once.
        /// </summary>
        [Test]
        public async Task CancelRentalAsync_ShouldCancelAndReleaseVehicle_WhenRentalIsActive()
        {
            var rentalId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();
            var vehicle = new TbVehicle { Id = vehicleId, Status = VehicleStatus.rented.ToString() };
            var rental = new TbRental
            {
                Id = rentalId,
                Status = RentalStatus.active.ToString(),
                VehicleId = vehicleId,
                Vehicle = vehicle
            };

            _repositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId))
                           .ReturnsAsync(rental);
            
            _repositoryMock.Setup(r => r.SaveChangesAsync())
                           .Returns(Task.CompletedTask);

            var result = await _service.CancelRentalAsync(rentalId);

            Assert.That(result.Status, Is.EqualTo(RentalStatus.canceled.ToString()));
            Assert.That(vehicle.Status, Is.EqualTo(VehicleStatus.available.ToString())); 
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        /// <summary>
        /// Ensures that attempting to cancel a rental that is not active throws
        /// an InvalidOperationException with the correct status information.
        /// </summary>
        [Test]
        public void CancelRentalAsync_ShouldThrow_WhenRentalIsNotActive()
        {
            var rentalId = Guid.NewGuid();
            var rental = new TbRental
            {
                Id = rentalId,
                Status = RentalStatus.completed.ToString() 
            };

            _repositoryMock.Setup(r => r.GetRentalByIdAsync(rentalId))
                           .ReturnsAsync(rental);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.CancelRentalAsync(rentalId));

            Assert.That(ex.Message, Does.Contain("It is not possible to cancel a rental with status 'completed'"));
        }

        /// <summary>
        /// Validates that an empty UserId is rejected before calling the repository,
        /// throwing an InvalidOperationException with the correct message.
        /// </summary>
        [Test]
        public void SearchRentalsByUserAsync_UserIdEmpty_ThrowsException()
        {
            var userId = Guid.Empty;
            string? status = RentalStatus.active.ToString();
            var page = 1;

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.SearchRentalsByUserAsync(userId, status, page));

            Assert.That(ex!.Message, Is.EqualTo(Messages.UserIdRequired));

            _repositoryMock.Verify(r =>
                r.SearchRentalsByUserAsync(It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public async Task GetRentalsAsync_ShouldMapEmptyStrings_WhenUserOrVehicleIsNull()
        {
            var rentalsFromDb = new List<TbRental>
    {
        new TbRental
        {
            Id = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            ExpectedEndDate = DateTime.UtcNow.AddDays(1),
            Status = RentalStatus.active.ToString(),
            User = null,
            Vehicle = null
        }
    };

            _repositoryMock.Setup(r => r.GetRentalsAsync())
                           .ReturnsAsync(rentalsFromDb);

            var result = await _service.GetRentalsAsync();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].UserName, Is.EqualTo(""));
            Assert.That(result[0].VehicleModel, Is.EqualTo(""));
        }

        [Test]
        public void GetRentalByIdAsync_ShouldThrow_WhenIdIsEmpty()
        {
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetRentalByIdAsync(Guid.Empty));

            Assert.That(ex!.Message, Is.EqualTo("The rental identifier is required."));
        }


        [Test]
        public void CreateRentalAsync_ShouldThrow_WhenUserNotFound()
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

            _repositoryMock.Setup(r => r.GetUserById(userId))
                           .ReturnsAsync((TbUser?)null);

            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.CreateRentalAsync(dto));

            Assert.That(ex!.Message, Is.EqualTo(Messages.UserNotFound));

            _repositoryMock.Verify(r => r.GetVehicleById(It.IsAny<Guid>()), Times.Never);
            _repositoryMock.Verify(r => r.CreateRentalAsync(It.IsAny<TbRental>()), Times.Never);
        }
    }
}