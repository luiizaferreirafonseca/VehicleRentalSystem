using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Resources;
using VehicleRentalSystem.Services.interfaces;

namespace VehicleRentalSystem.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<UserResponseDTO>> GetAllUsersAsync()
        {
            var users = await _repository.GetAllUsersAsync();

            if (users.Any(u => string.IsNullOrWhiteSpace(u.Name)))
                throw new InvalidOperationException(Messages.UserNameMissing);

            if (users.Any(u => string.IsNullOrWhiteSpace(u.Email)))
                throw new InvalidOperationException(Messages.UserEmailMissing);

            return users.Select(u => new UserResponseDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Active = u.Active,
                Rentals = u.TbRentals.Select(r => new UserRentalItemDTO
                {
                    RentalId = r.Id,
                    VehicleId = r.VehicleId
                }).ToList()
            }).ToList();
        }
    }
}
