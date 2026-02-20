using Moq;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Resources;
using VehicleRentalSystem.Validator;

namespace VehicleSystem.Tests.Validators
{
    [TestFixture]
    [Category("Validators")]
    public class RentalValidatorTests
    {
        private Mock<IRentalRepository> _repositoryMock;

        [SetUp]
        public void SetUp()
        {
            _repositoryMock = new Mock<IRentalRepository>();
        }

        #region CheckIfUserExistsAsync

        /// <summary>
        /// Validates that a valid user ID returns the corresponding TbUser entity
        /// from the repository with all properties correctly mapped.
        /// </summary>
        [Test]
        public async Task CheckIfUserExistsAsync_UserExists_ReturnsUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedUser = new TbUser { Id = userId, Name = "John Doe", Email = "john@test.com" };

            _repositoryMock
                .Setup(r => r.GetUserById(userId))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await RentalValidator.CheckIfUserExistsAsync(_repositoryMock.Object, userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(userId));
            Assert.That(result.Name, Is.EqualTo(expectedUser.Name));
        }

        /// <summary>
        /// Ensures that a non-existent user ID throws a KeyNotFoundException.
        /// </summary>
        [Test]
        public void CheckIfUserExistsAsync_UserNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _repositoryMock
                .Setup(r => r.GetUserById(userId))
                .ReturnsAsync((TbUser?)null);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await RentalValidator.CheckIfUserExistsAsync(_repositoryMock.Object, userId));
        }

        /// <summary>
        /// Validates that the KeyNotFoundException message matches
        /// the Messages.UserNotFound constant when the user does not exist.
        /// </summary>
        [Test]
        public async Task CheckIfUserExistsAsync_UserNotFound_ThrowsWithCorrectMessage()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _repositoryMock
                .Setup(r => r.GetUserById(userId))
                .ReturnsAsync((TbUser?)null);

            // Act
            var ex = Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await RentalValidator.CheckIfUserExistsAsync(_repositoryMock.Object, userId));

            // Assert
            Assert.That(ex!.Message, Is.EqualTo(Messages.UserNotFound));

            await Task.CompletedTask;
        }

        /// <summary>
        /// Verifies that the repository's GetUserById is called exactly once
        /// with the correct user ID.
        /// </summary>
        [Test]
        public async Task CheckIfUserExistsAsync_AnyUserId_CallsRepositoryOnce()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new TbUser { Id = userId, Name = "Jane Doe", Email = "jane@test.com" };

            _repositoryMock
                .Setup(r => r.GetUserById(userId))
                .ReturnsAsync(user);

            // Act
            await RentalValidator.CheckIfUserExistsAsync(_repositoryMock.Object, userId);

            // Assert
            _repositoryMock.Verify(r => r.GetUserById(userId), Times.Once);
        }

        /// <summary>
        /// Ensures the repository is called exactly once even when a KeyNotFoundException
        /// is thrown due to a missing user.
        /// </summary>
        [Test]
        public void CheckIfUserExistsAsync_UserNotFound_RepositoryCalledOnceBeforeException()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _repositoryMock
                .Setup(r => r.GetUserById(userId))
                .ReturnsAsync((TbUser?)null);

            // Act
            Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await RentalValidator.CheckIfUserExistsAsync(_repositoryMock.Object, userId));

            // Assert
            _repositoryMock.Verify(r => r.GetUserById(userId), Times.Once);
        }

        #endregion

        #region CheckExpectedEndDateIsAfterStart

        /// <summary>
        /// Validates the happy path: an end date strictly after the start date
        /// does not throw any exception.
        /// </summary>
        [Test]
        public void CheckExpectedEndDateIsAfterStart_EndDateAfterStartDate_DoesNotThrow()
        {
            // Arrange
            var dto = new RentalCreateDTO
            {
                StartDate = new DateTime(2026, 02, 10, 0, 0, 0, DateTimeKind.Utc),
                ExpectedEndDate = new DateTime(2026, 02, 15, 0, 0, 0, DateTimeKind.Utc)
            };

            // Act & Assert
            Assert.DoesNotThrow(() => RentalValidator.CheckExpectedEndDateIsAfterStart(dto));
        }

        /// <summary>
        /// Ensures that equal start and end dates are treated as an invalid range
        /// and throw an InvalidOperationException.
        /// </summary>
        [Test]
        public void CheckExpectedEndDateIsAfterStart_EndDateEqualToStartDate_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new RentalCreateDTO
            {
                StartDate = new DateTime(2026, 02, 10, 0, 0, 0, DateTimeKind.Utc),
                ExpectedEndDate = new DateTime(2026, 02, 10, 0, 0, 0, DateTimeKind.Utc)
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                RentalValidator.CheckExpectedEndDateIsAfterStart(dto));
        }

        /// <summary>
        /// Ensures that an end date prior to the start date throws
        /// an InvalidOperationException.
        /// </summary>
        [Test]
        public void CheckExpectedEndDateIsAfterStart_EndDateBeforeStartDate_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new RentalCreateDTO
            {
                StartDate = new DateTime(2026, 02, 15, 0, 0, 0, DateTimeKind.Utc),
                ExpectedEndDate = new DateTime(2026, 02, 10, 0, 0, 0, DateTimeKind.Utc)
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                RentalValidator.CheckExpectedEndDateIsAfterStart(dto));
        }

        /// <summary>
        /// Validates that the exception message matches Messages.ExpectedEndDateInvalid
        /// when the end date equals the start date.
        /// </summary>
        [Test]
        public void CheckExpectedEndDateIsAfterStart_EndDateEqualToStartDate_ThrowsWithCorrectMessage()
        {
            // Arrange
            var dto = new RentalCreateDTO
            {
                StartDate = new DateTime(2026, 02, 10, 0, 0, 0, DateTimeKind.Utc),
                ExpectedEndDate = new DateTime(2026, 02, 10, 0, 0, 0, DateTimeKind.Utc)
            };

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() =>
                RentalValidator.CheckExpectedEndDateIsAfterStart(dto));

            // Assert
            Assert.That(ex!.Message, Is.EqualTo(Messages.ExpectedEndDateInvalid));
        }

        /// <summary>
        /// Validates that the exception message matches Messages.ExpectedEndDateInvalid
        /// when the end date is before the start date.
        /// </summary>
        [Test]
        public void CheckExpectedEndDateIsAfterStart_EndDateBeforeStartDate_ThrowsWithCorrectMessage()
        {
            // Arrange
            var dto = new RentalCreateDTO
            {
                StartDate = new DateTime(2026, 02, 15, 0, 0, 0, DateTimeKind.Utc),
                ExpectedEndDate = new DateTime(2026, 02, 10, 0, 0, 0, DateTimeKind.Utc)
            };

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() =>
                RentalValidator.CheckExpectedEndDateIsAfterStart(dto));

            // Assert
            Assert.That(ex!.Message, Is.EqualTo(Messages.ExpectedEndDateInvalid));
        }

        /// <summary>
        /// Verifies that a null StartDate does not trigger the validation rule,
        /// since the lifted comparison operator returns false for null operands.
        /// </summary>
        [Test]
        public void CheckExpectedEndDateIsAfterStart_StartDateIsNull_DoesNotThrow()
        {
            // Arrange
            var dto = new RentalCreateDTO
            {
                StartDate = null,
                ExpectedEndDate = new DateTime(2026, 02, 10, 0, 0, 0, DateTimeKind.Utc)
            };

            // Act & Assert
            Assert.DoesNotThrow(() => RentalValidator.CheckExpectedEndDateIsAfterStart(dto));
        }

        #endregion
    }
}
