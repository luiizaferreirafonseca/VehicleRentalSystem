using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;

namespace VehicleRentalSystem.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly PostgresContext _postgresContext;

        public VehicleRepository(PostgresContext postgresContext)
        {
            _postgresContext = postgresContext;
        }

        public async Task<bool> ExistsByLicensePlateAsync(string licensePlate)
        {
            return await _postgresContext.TbVehicles
                .AnyAsync(v => v.LicensePlate == licensePlate);
        }

        public async Task<TbVehicle> CreateVehicleAsync(TbVehicle vehicle)
        {
            await _postgresContext.TbVehicles.AddAsync(vehicle);
            await _postgresContext.SaveChangesAsync();
            return vehicle;
        }
        public async Task<TbVehicle?> GetVehicleByIdAsync(Guid id)
        {
            return await _postgresContext.TbVehicles
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<bool> DeleteVehicleAsync(TbVehicle vehicle)
        {
            _postgresContext.TbVehicles.Remove(vehicle);
            var affected = await _postgresContext.SaveChangesAsync();
            return affected > 0;
        }
    }
}
