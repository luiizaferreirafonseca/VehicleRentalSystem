using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories.interfaces
{
    public interface IPaymentRepository
    {
        Task AddPaymentAsync(TbPayment payment);
        Task<decimal> GetTotalPaymentsAsync(Guid rentalId);
        Task<IEnumerable<TbPayment>> GetPaymentsByRentalIdAsync(Guid rentalId);
        Task SaveChangesAsync();
    }
}
