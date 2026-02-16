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

        public async Task<IEnumerable<TbPayment>> GetAllPaymentsAsync(Guid? rentalId, string? method, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.TbPayments.AsQueryable();

            if (rentalId.HasValue)
                query = query.Where(p => p.RentalId == rentalId.Value);

            if (!string.IsNullOrWhiteSpace(method))
            {
                var methodFilter = method.Trim().ToLower();
                query = query.Where(p => p.PaymentMethod.ToLower() == methodFilter);
            }

            if (startDate.HasValue)
            {
                var startUtc = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
                query = query.Where(p => p.PaymentDate >= startUtc);
            }

            if (endDate.HasValue)
            {
                var endUtc = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
                query = query.Where(p => p.PaymentDate <= endUtc);
            }

            return await query.ToListAsync();
        }
    }
}
