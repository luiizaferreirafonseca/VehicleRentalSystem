using Moq;
using NUnit.Framework;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Services;

namespace VehicleSystem.Tests
{
    [TestFixture]
    public class RentalRatingTests
    {
        private Mock<IRatingRepository> _ratingRepoMock;
        private Mock<IRentalRepository> _rentalRepoMock;
        private RatingService _service;

        [SetUp]
        public void SetUp()
        {
            _ratingRepoMock = new Mock<IRatingRepository>();
            _rentalRepoMock = new Mock<IRentalRepository>();
            _service = new RatingService(_ratingRepoMock.Object, _rentalRepoMock.Object);
        }

        [Test]
        public async Task EvaluateRentalAsync_ValidData_ReturnsTrue()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var dto = new RatingCreateDTO { RentalId = rentalId, Rating = 5, Comment = "Ótimo!" };
            var rental = new TbRental { Id = rentalId, Status = "Finalizada" };

            _rentalRepoMock.Setup(r => r.GetRentalByIdAsync(rentalId)).ReturnsAsync(rental);
            _ratingRepoMock.Setup(r => r.GetByRentalIdAsync(rentalId)).ReturnsAsync((TbRating)null);

            // Act
            var result = await _service.EvaluateRentalAsync(dto);

            // Assert
            Assert.IsTrue(result);
            _ratingRepoMock.Verify(r => r.AddAsync(It.IsAny<TbRating>()), Times.Once);
            _ratingRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void EvaluateRentalAsync_InvalidScore_ThrowsException()
        {
            // Arrange
            var dto = new RatingCreateDTO { Rating = 6 }; 

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.EvaluateRentalAsync(dto));
            Assert.That(ex.Message, Is.EqualTo("A nota deve estar entre 1 e 5."));
        }

        [Test]
        public void EvaluateRentalAsync_RentalNotFinished_ThrowsException()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var dto = new RatingCreateDTO { RentalId = rentalId, Rating = 4 };
            var rental = new TbRental { Id = rentalId, Status = "Ativa" };

            _rentalRepoMock.Setup(r => r.GetRentalByIdAsync(rentalId)).ReturnsAsync(rental);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.EvaluateRentalAsync(dto));
            Assert.That(ex.Message, Is.EqualTo("Você só pode avaliar locações que já foram finalizadas."));
        }

        [Test]
        public void EvaluateRentalAsync_AlreadyRated_ThrowsException()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var dto = new RatingCreateDTO { RentalId = rentalId, Rating = 4 };
            var rental = new TbRental { Id = rentalId, Status = "Finalizada" };
            var existingRating = new TbRating();

            _rentalRepoMock.Setup(r => r.GetRentalByIdAsync(rentalId)).ReturnsAsync(rental);
            _ratingRepoMock.Setup(r => r.GetByRentalIdAsync(rentalId)).ReturnsAsync(existingRating);

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.EvaluateRentalAsync(dto));
            Assert.That(ex.Message, Is.EqualTo("Esta locação já foi avaliada anteriormente."));
        }
    }
}