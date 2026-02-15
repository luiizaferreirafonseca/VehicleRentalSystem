using System.Threading.Tasks;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Services.interfaces
{
    public interface IVehicleService
    {
        Task<VehicleResponseDTO> CreateVehicleAsync(VehicleCreateDTO dto);
        Task RemoveVehicleAsync(Guid vehicleId);
        Task<List<VehicleResponseDTO>> SearchVehiclesAsync(string? status, int page);
    }
}

