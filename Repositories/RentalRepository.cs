using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;

namespace VehicleRentalSystem.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly PostgresContext _postgresContext;

        public RentalRepository(PostgresContext postgresContext)
        {
            _postgresContext = postgresContext;
        }

        public List<TbRental> SelectRentals()
        {
            return _postgresContext.TbRentals
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .ToList();
        }


        public TbRental? SelectRentalById(Guid id)
        {
            return _postgresContext.TbRentals
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .FirstOrDefault(r => r.Id == id);
        }

        public async Task<TbUser?> GetUserById(Guid id)
        {
            return await _postgresContext.TbUsers.FindAsync(id);
        }

        public async Task<TbVehicle?> GetVehicleById(Guid id)
        {
            return await _postgresContext.TbVehicles.FindAsync(id);
        }

        public async Task<TbRental> CreateRentalAsync(TbRental rental)
        {
            await _postgresContext.TbRentals.AddAsync(rental);
            await _postgresContext.SaveChangesAsync();
            return rental;
        }

        public async Task<bool> UpdateVehicleStatusAsync(Guid vehicleId, string status)
        {
            var vehicle = await _postgresContext.TbVehicles.FindAsync(vehicleId);
            if (vehicle == null)
                return false;

            vehicle.Status = status;
            await _postgresContext.SaveChangesAsync();
            return true;
        }


        public async Task<TbRental?> GetRentalByIdAsync(Guid id)
        {
            return await _postgresContext.TbRentals
                .Include(r => r.Vehicle)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _postgresContext.SaveChangesAsync();
        }
    }
}