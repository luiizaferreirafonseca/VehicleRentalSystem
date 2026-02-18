using System;
using NUnit.Framework;
using VehicleRentalSystem.DTO;

namespace VehicleSystem.Tests.DTOs
{
    [TestFixture]
    [Category("DTOs")]
    public class AccessoryDtoTests
    {
        [Test]
        public void AccessoryCreateDto_Defaults_AreExpected()
        {
            var dto = new AccessoryCreateDto();

            Assert.Multiple(() =>
            {
                Assert.That(dto.Name, Is.EqualTo(string.Empty));
                Assert.That(dto.DailyRate, Is.EqualTo(0m));
            });
        }

        [Test]
        public void AccessoryReportDto_Defaults_AreExpected()
        {
            var dto = new AccessoryReportDto();

            Assert.Multiple(() =>
            {
                Assert.That(dto.Id, Is.EqualTo(Guid.Empty));
                Assert.That(dto.Name, Is.EqualTo(string.Empty));
                Assert.That(dto.Quantity, Is.EqualTo(0));
                Assert.That(dto.UnitPrice, Is.EqualTo(0m));
                Assert.That(dto.TotalPrice, Is.EqualTo(0m));
                Assert.That(dto.DailyRate, Is.EqualTo(0m));
                Assert.That(dto.StartDate, Is.EqualTo(default(DateTime)));
                Assert.That(dto.EndDate, Is.EqualTo(default(DateTime)));
            });
        }

        [Test]
        public void AccessoryReportDto_CanSetProperties()
        {
            var id = Guid.NewGuid();
            var start = DateTime.Now.Date;
            var end = start.AddDays(2);
            var dto = new AccessoryReportDto
            {
                Id = id,
                Name = "GPS",
                Quantity = 2,
                UnitPrice = 5.5m,
                TotalPrice = 11m,
                DailyRate = 5.5m,
                StartDate = start,
                EndDate = end
            };

            Assert.Multiple(() =>
            {
                Assert.That(dto.Id, Is.EqualTo(id));
                Assert.That(dto.Name, Is.EqualTo("GPS"));
                Assert.That(dto.Quantity, Is.EqualTo(2));
                Assert.That(dto.UnitPrice, Is.EqualTo(5.5m));
                Assert.That(dto.TotalPrice, Is.EqualTo(11m));
                Assert.That(dto.DailyRate, Is.EqualTo(5.5m));
                Assert.That(dto.StartDate, Is.EqualTo(start));
                Assert.That(dto.EndDate, Is.EqualTo(end));
            });
        }

        [Test]
        public void AccessoryCreateDto_CanSetProperties()
        {
            var dto = new AccessoryCreateDto
            {
                Name = "GPS",
                DailyRate = 12.5m
            };

            Assert.Multiple(() =>
            {
                Assert.That(dto.Name, Is.EqualTo("GPS"));
                Assert.That(dto.DailyRate, Is.EqualTo(12.5m));
            });
        }

        [Test]
        public void AccessoryResponseDto_Defaults_AreExpected()
        {
            var dto = new AccessoryResponseDto();

            Assert.Multiple(() =>
            {
                Assert.That(dto.Id, Is.EqualTo(Guid.Empty));
                Assert.That(dto.Name, Is.EqualTo(string.Empty));
                Assert.That(dto.DailyRate, Is.EqualTo(0m));
            });
        }

        [Test]
        public void AccessoryResponseDto_CanSetProperties()
        {
            var id = Guid.NewGuid();
            var dto = new AccessoryResponseDto
            {
                Id = id,
                Name = "Cadeira Bebê",
                DailyRate = 20m
            };

            Assert.Multiple(() =>
            {
                Assert.That(dto.Id, Is.EqualTo(id));
                Assert.That(dto.Name, Is.EqualTo("Cadeira Bebê"));
                Assert.That(dto.DailyRate, Is.EqualTo(20m));
            });
        }
    }
}
