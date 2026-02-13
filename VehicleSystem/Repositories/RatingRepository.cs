using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;

namespace VehicleRentalSystem.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly PostgresContext _context;

        public RatingRepository(PostgresContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TbRating rating)
        {
            await _context.TbRatings.AddAsync(rating);
        }

        public async Task<TbRating?> GetByRentalIdAsync(Guid rentalId)
        {
            return await _context.TbRatings
                .FirstOrDefaultAsync(r => r.RentalId == rentalId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}