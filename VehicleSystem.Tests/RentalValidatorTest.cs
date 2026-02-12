using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Resources;
using VehicleRentalSystem.Validator;

[TestFixture]
public class RentalValidatorTest
{
    [Test]
    public void Should_Throw_When_Expected_End_Date_Is_Less_Or_Equal_Start_Date()
    {
        var dto = new RentalCreateDTO
        {
            StartDate = new DateTime(2026, 02, 10),
            ExpectedEndDate = new DateTime(2026, 02, 10)
        };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            RentalValidator.CheckExpectedEndDateIsAfterStart(dto)
        );

        Assert.That(ex!.Message, Is.EqualTo(Messages.ExpectedEndDateInvalid));
    }
}
