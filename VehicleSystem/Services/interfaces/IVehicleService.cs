using System.Threading.Tasks;
using VehicleRentalSystem.DTO;

namespace VehicleRentalSystem.Services.interfaces
{
    public interface IVehicleService
    {
        Task<VehicleResponseDTO> CreateVehicleAsync(VehicleCreateDTO dto);
    }
}
