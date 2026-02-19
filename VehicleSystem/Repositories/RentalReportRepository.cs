using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;

namespace VehicleRentalSystem.Repositories
{
    public class RentalReportRepository : IRentalReportRepository
    {
        private readonly PostgresContext _context;

        public RentalReportRepository(PostgresContext context)
        {
            _context = context;
        }

        public async Task<TbRental?> GetRentalWithDetailsAsync(Guid rentalId)
        {//
            // Performs the necessary joins using Include to load related data
            return await _context.TbRentals
                .AsNoTracking()
                .Include(r => r.Vehicle)   // Join with the Vehicles table
                .Include(r => r.User)  // Join with the Users table (customer)
                .Include(r => r.TbPayments) // Join with the Payments table
                .Include(r => r.TbRentalAccessories) // Join with the rental-related accessories table
                    .ThenInclude(ra => ra.Accessory) // Includes the accessory details
                .FirstOrDefaultAsync(r => r.Id == rentalId);
        }
    }
}