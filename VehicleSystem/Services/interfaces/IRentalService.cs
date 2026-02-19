using System.Threading.Tasks;
using VehicleRentalSystem.DTO;

namespace VehicleRentalSystem.Services.interfaces
{
    public interface IRentalService
    {
        Task<List<RentalResponseDTO>> GetRentalsAsync();
        Task<RentalResponseDTO> GetRentalByIdAsync(Guid id);
        Task<RentalResponseDTO> CreateRentalAsync(RentalCreateDTO dto);
        Task<RentalResponseDTO> CancelRentalAsync(Guid id);
        Task<RentalResponseDTO> UpdateRentalDatesAsync(Guid id, UpdateRentalDTO updateDto);
        Task<RentalResponseDTO> ReturnRentalAsync(Guid id);
        Task<List<RentalResponseDTO>> SearchRentalsByUserAsync(Guid userId, string? status, int page);
    }
}