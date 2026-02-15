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

        public async Task DeleteVehicleAsync(TbVehicle vehicle)
        {
            _postgresContext.TbVehicles.Remove(vehicle);
            await _postgresContext.SaveChangesAsync();
        }


        public async Task<List<TbVehicle>> SearchVehiclesAsync(string? status, int page)
        {
            const int pageSize = 5;

            if (page < 1)
                page = 1;

            var query = _postgresContext.TbVehicles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                var correctStatusName = status.Trim().ToLower();
                query = query.Where(v => v.Status.ToLower() == correctStatusName);
            }

            return await query
                .OrderBy(v => v.Brand)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

    }


}
