using System;
using NUnit.Framework;
using VehicleRentalSystem.DTO;

namespace VehicleSystem.Tests.DTOs
{
    [TestFixture]
    [Category("DTOs")]
    public class RentalCreateDtoTests
    {
        [Test]
        public void RentalCreateDTO_Defaults_AreExpected()
        {
            var dto = new RentalCreateDTO();

            Assert.Multiple(() =>
            {
                Assert.That(dto.UserId, Is.EqualTo(Guid.Empty));
                Assert.That(dto.VehicleId, Is.EqualTo(Guid.Empty));

                Assert.That(dto.StartDate, Is.Null);

                Assert.That(dto.ExpectedEndDate, Is.EqualTo(default(DateTime)));
            });
        }

        [Test]
        public void RentalCreateDTO_CanSetProperties()
        {
            var userId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();
            var start = new DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc);
            var expectedEnd = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

            var dto = new RentalCreateDTO
            {
                UserId = userId,
                VehicleId = vehicleId,
                StartDate = start,
                ExpectedEndDate = expectedEnd
            };

            Assert.Multiple(() =>
            {
                Assert.That(dto.UserId, Is.EqualTo(userId));
                Assert.That(dto.VehicleId, Is.EqualTo(vehicleId));
                Assert.That(dto.StartDate, Is.EqualTo(start));
                Assert.That(dto.ExpectedEndDate, Is.EqualTo(expectedEnd));
            });
        }
    }
}