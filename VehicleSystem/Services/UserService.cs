using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Models;
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

        public async Task<UserResponseDTO> CreateUserAsync(UserCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException(Messages.UserNameMissing);

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new InvalidOperationException(Messages.UserEmailMissing);

            var emailExists = await _repository.ExistsByEmailAsync(dto.Email);
            if (emailExists)
                throw new InvalidOperationException("Este e-mail já está cadastrado no sistema.");

            var newUser = new TbUser
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim().ToLower(),
                Active = true
            };

            var createdUser = await _repository.CreateUserAsync(newUser);

            return new UserResponseDTO
            {
                Id = createdUser.Id,
                Name = createdUser.Name,
                Email = createdUser.Email,
                Active = createdUser.Active,
                Rentals = new List<UserRentalItemDTO>()
            };
        }
    }
}