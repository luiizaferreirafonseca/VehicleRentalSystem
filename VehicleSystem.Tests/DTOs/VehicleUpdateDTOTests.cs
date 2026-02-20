using NUnit.Framework;
using System;
using VehicleRentalSystem.DTO;

namespace VehicleSystem.Tests.DTOs
{
    [TestFixture]
    [Category("DTOs")]
    public class VehicleUpdateDtoTests
    {
        [Test]
        public void VehicleUpdateDTO_Defaults_AreExpected()
        {
            // Arrange / Act
            var dto = new VehicleUpdateDTO();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(dto.DailyRate, Is.EqualTo(0m));
                Assert.That(dto.Year, Is.EqualTo(0));
                Assert.That(dto.Status, Is.EqualTo(string.Empty));
            });
        }

        [Test]
        public void VehicleUpdateDTO_CanSetProperties()
        {
            // Arrange
            var dto = new VehicleUpdateDTO
            {
                DailyRate = 250.50m,
                Year = 2024,
                Status = "maintenance"
            };

            // Act / Assert
            Assert.Multiple(() =>
            {
                Assert.That(dto.DailyRate, Is.EqualTo(250.50m));
                Assert.That(dto.Year, Is.EqualTo(2024));
                Assert.That(dto.Status, Is.EqualTo("maintenance"));
            });
        }

        [Test]
        public void VehicleUpdateDTO_Status_AllowsNullAssignment()
        {
            // Arrange
            var dto = new VehicleUpdateDTO();

            // Act
            dto.Status = null!; // permitido em runtime, apesar do = string.Empty

            // Assert
            Assert.That(dto.Status, Is.Null);
        }

        [Test]
        public void VehicleUpdateDTO_CanSetStatusToWhitespace()
        {
            // Arrange
            var dto = new VehicleUpdateDTO();

            // Act
            dto.Status = "   ";

            // Assert
            Assert.That(dto.Status, Is.EqualTo("   "));
        }

        [Test]
        public void VehicleUpdateDTO_CanSetYearAndDailyRateToNegativeValues()
        {
            // Arrange
            var dto = new VehicleUpdateDTO();

            // Act
            dto.Year = -1;
            dto.DailyRate = -10m;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(dto.Year, Is.EqualTo(-1));
                Assert.That(dto.DailyRate, Is.EqualTo(-10m));
            });
        }
    }
}