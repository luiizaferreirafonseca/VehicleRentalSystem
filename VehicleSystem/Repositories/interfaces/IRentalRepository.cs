using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories.interfaces
{
    public interface IRentalRepository
    {
        List<TbRental> SelectRentals();

        TbRental? SelectRentalById(Guid id);

        Task<TbUser?> GetUserById(Guid id);

        Task<TbVehicle?> GetVehicleById(Guid id);

        Task<TbRental> CreateRentalAsync(TbRental rental);
        
        Task<bool> UpdateVehicleStatusAsync(Guid vehicleId, string status);

        Task<TbRental?> GetRentalByIdAsync(Guid id);

        Task AddPaymentAsync(TbPayment payment);

        Task<decimal> GetTotalPaymentsAsync(Guid rentalId);

        Task UpdateAsync(TbRental rental);

        Task SaveChangesAsync();
    }
}