using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories.interfaces
{
    public interface IVehicleRepository
    {
        Task<bool> ExistsByLicensePlateAsync(string licensePlate);
        Task<TbVehicle> CreateVehicleAsync(TbVehicle vehicle);
        Task<TbVehicle?> GetVehicleByIdAsync(Guid id);
        Task DeleteVehicleAsync(TbVehicle vehicle);
        Task<List<TbVehicle>> SearchVehiclesAsync(string? status, int page);
    }
}