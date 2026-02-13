using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories.interfaces
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
    }
}
