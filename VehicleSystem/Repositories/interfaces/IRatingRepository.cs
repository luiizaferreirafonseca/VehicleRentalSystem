using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories.interfaces
{
    public interface IRatingRepository
    {
        Task AddAsync(TbRating rating);
        Task<TbRating?> GetByRentalIdAsync(Guid rentalId);
        Task SaveChangesAsync(); 
    }
}