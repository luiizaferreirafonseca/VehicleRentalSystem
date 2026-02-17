using VehicleRentalSystem.DTO;

namespace VehicleRentalSystem.Services.interfaces
{
    public interface IUserService
    {
        Task<List<UserResponseDTO>> GetAllUsersAsync();
        Task<UserResponseDTO> CreateUserAsync(UserCreateDTO dto);
    }
}