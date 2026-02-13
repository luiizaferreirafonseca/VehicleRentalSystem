using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories
{
    public interface IVehicleRepository
    {
        Task<bool> ExistsByLicensePlateAsync(string licensePlate);
        Task<TbVehicle> CreateVehicleAsync(TbVehicle vehicle);
    }
}
