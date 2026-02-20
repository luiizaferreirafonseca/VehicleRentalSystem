using NUnit.Framework;
using VehicleRentalSystem.DTO;

namespace VehicleSystem.Tests.DTOs
{
    [TestFixture]
    [Category("DTOs")]
    public class UserCreateDtoTests
    {
        [Test]
        public void UserCreateDTO_Defaults_AreExpected()
        {
            var dto = new UserCreateDTO();

            Assert.Multiple(() =>
            {
                Assert.That(dto.Name, Is.EqualTo(string.Empty));
                Assert.That(dto.Email, Is.EqualTo(string.Empty));
            });
        }

        [Test]
        public void UserCreateDTO_CanSetProperties()
        {
            var dto = new UserCreateDTO
            {
                Name = "Maria Silva",
                Email = "maria@email.com"
            };

            Assert.Multiple(() =>
            {
                Assert.That(dto.Name, Is.EqualTo("Maria Silva"));
                Assert.That(dto.Email, Is.EqualTo("maria@email.com"));
            });
        }
    }
}