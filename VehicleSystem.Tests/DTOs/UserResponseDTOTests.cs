using NUnit.Framework;
using System;
using System.Collections.Generic;
using VehicleRentalSystem.DTO;

namespace VehicleSystem.Tests.DTOs
{
    [TestFixture]
    [Category("DTOs")]
    public class UserResponseDtoTests
    {
        [Test]
        public void UserResponseDTO_DefaultValues_ShouldBeCorrect()
        {
            // Arrange / Act
            var dto = new UserResponseDTO();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(dto.Id, Is.EqualTo(Guid.Empty));
                Assert.That(dto.Name, Is.Null);       // string sem valor inicial
                Assert.That(dto.Email, Is.Null);      // string sem valor inicial
                Assert.That(dto.Active, Is.Null);     // bool?
                Assert.That(dto.Rentals, Is.Not.Null);
                Assert.That(dto.Rentals, Is.Empty);
            });
        }

        [Test]
        public void UserResponseDTO_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var rentals = new List<UserRentalItemDTO>
            {
                new UserRentalItemDTO { RentalId = Guid.NewGuid(), VehicleId = Guid.NewGuid() }
            };

            var dto = new UserResponseDTO
            {
                Id = id,
                Name = "Luiza",
                Email = "luiza@email.com",
                Active = true,
                Rentals = rentals
            };

            // Act / Assert
            Assert.Multiple(() =>
            {
                Assert.That(dto.Id, Is.EqualTo(id));
                Assert.That(dto.Name, Is.EqualTo("Luiza"));
                Assert.That(dto.Email, Is.EqualTo("luiza@email.com"));
                Assert.That(dto.Active, Is.True);
                Assert.That(dto.Rentals, Is.EqualTo(rentals));
                Assert.That(dto.Rentals.Count, Is.EqualTo(1));
            });
        }

        [Test]
        public void UserResponseDTO_ShouldAllowActiveToBeNull()
        {
            // Arrange
            var dto = new UserResponseDTO
            {
                Active = null
            };

            // Assert
            Assert.That(dto.Active, Is.Null);
        }

        [Test]
        public void UserResponseDTO_ShouldAllowAddingRentals()
        {
            // Arrange
            var dto = new UserResponseDTO();

            var rental = new UserRentalItemDTO
            {
                RentalId = Guid.NewGuid(),
                VehicleId = Guid.NewGuid()
            };

            // Act
            dto.Rentals.Add(rental);

            // Assert
            Assert.That(dto.Rentals.Count, Is.EqualTo(1));
            Assert.That(dto.Rentals[0], Is.EqualTo(rental));
        }

        [Test]
        public void UserResponseDTO_ShouldSupportMultipleRentals()
        {
            // Arrange
            var dto = new UserResponseDTO();

            dto.Rentals.Add(new UserRentalItemDTO
            {
                RentalId = Guid.NewGuid(),
                VehicleId = Guid.NewGuid()
            });

            dto.Rentals.Add(new UserRentalItemDTO
            {
                RentalId = Guid.NewGuid(),
                VehicleId = Guid.NewGuid()
            });

            // Assert
            Assert.That(dto.Rentals, Has.Count.EqualTo(2));
        }
    }
}