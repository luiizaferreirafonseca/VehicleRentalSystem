using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories.interfaces
{
    public interface IRentalReportRepository
    {
        Task<TbRental?> GetRentalWithDetailsAsync(Guid rentalId);
    }
}