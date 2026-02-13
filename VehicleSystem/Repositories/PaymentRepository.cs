using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;

namespace VehicleRentalSystem.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PostgresContext _context;

        public PaymentRepository(PostgresContext context)
        {
            _context = context;
        }

        public async Task AddPaymentAsync(TbPayment payment)
        {
            await _context.TbPayments.AddAsync(payment);
        }

        public async Task<decimal> GetTotalPaymentsAsync(Guid rentalId)
        {
            var total = await _context.TbPayments
                .Where(p => p.RentalId == rentalId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            return total;
        }

        public async Task<IEnumerable<TbPayment>> GetPaymentsByRentalIdAsync(Guid rentalId)
        {
            return await _context.TbPayments
                .Where(p => p.RentalId == rentalId)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
