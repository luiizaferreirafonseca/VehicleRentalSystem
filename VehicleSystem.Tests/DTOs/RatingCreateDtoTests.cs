using System;
using NUnit.Framework;
using VehicleRentalSystem.DTO;

namespace VehicleSystem.Tests.DTOs
{
    [TestFixture]
    [Category("DTOs")]
    public class RatingCreateDtoTests
    {
        [Test]
        public void RatingCreateDTO_Defaults_AreExpected()
        {
            var dto = new RatingCreateDTO();

            Assert.Multiple(() =>
            {
                Assert.That(dto.RentalId, Is.EqualTo(Guid.Empty));
                Assert.That(dto.Rating, Is.EqualTo(0));
                Assert.That(dto.Comment, Is.Null);
            });
        }

        [Test]
        public void RatingCreateDTO_CanSetProperties()
        {
            var rentalId = Guid.NewGuid();

            var dto = new RatingCreateDTO
            {
                RentalId = rentalId,
                Rating = 5,
                Comment = "Excellent!"
            };

            Assert.Multiple(() =>
            {
                Assert.That(dto.RentalId, Is.EqualTo(rentalId));
                Assert.That(dto.Rating, Is.EqualTo(5));
                Assert.That(dto.Comment, Is.EqualTo("Excellent!"));
            });
        }

        [Test]
        public void RatingCreateDTO_CanSetNullComment()
        {
            var rentalId = Guid.NewGuid();

            var dto = new RatingCreateDTO
            {
                RentalId = rentalId,
                Rating = 3,
                Comment = null
            };

            Assert.Multiple(() =>
            {
                Assert.That(dto.RentalId, Is.EqualTo(rentalId));
                Assert.That(dto.Rating, Is.EqualTo(3));
                Assert.That(dto.Comment, Is.Null);
            });
        }
    }
}