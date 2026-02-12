using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Resources;

namespace VehicleRentalSystem.Validator
{
    public class RentalValidator
    {
        public static async Task<TbUser> CheckIfUserExistsAsync(IRentalRepository repository, Guid userId)
        {
            var user = await repository.GetUserById(userId);

            if (user == null)
                throw new KeyNotFoundException(Messages.UserNotFound);

            return user;
        }

        public static void CheckExpectedEndDateIsAfterStart(RentalCreateDTO dto)
        {
            if (dto.ExpectedEndDate <= dto.StartDate)
                throw new InvalidOperationException(Messages.ExpectedEndDateInvalid);
        }

    }
}
