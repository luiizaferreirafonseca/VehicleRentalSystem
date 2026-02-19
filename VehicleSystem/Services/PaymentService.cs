using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Enums;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Resources;
using VehicleRentalSystem.Services.interfaces;

namespace VehicleRentalSystem.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IRentalRepository rentalRepository, IPaymentRepository paymentRepository)
        {
            _rentalRepository = rentalRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentResponseDto> RegisterPaymentAsync(Guid rentalId, PaymentCreateDTO dto)
        {
            if (rentalId == Guid.Empty)
                throw new ArgumentException("The rental identifier is required.");

            var rental = await _rentalRepository.GetRentalByIdAsync(rentalId);

            if (rental == null)
                throw new KeyNotFoundException(Messages.RentalNotFound);

            if (rental.Status == RentalStatus.canceled.ToString())
                throw new InvalidOperationException("It is not possible to register a payment for a canceled rental.");

            if (dto.Amount <= 0)
                throw new InvalidOperationException("The payment amount must be greater than zero.");

            var totalPayments = await _paymentRepository.GetTotalPaymentsAsync(rentalId);
            var remaining = (rental.TotalAmount ?? 0m) - totalPayments;

            if (dto.Amount > remaining)
                throw new InvalidOperationException("The sum of payments cannot exceed the total rental amount.");

            var paymentMethodString = dto.PaymentMethod?.ToString().ToLower() ?? string.Empty;

            var payment = new TbPayment
            {
                Id = Guid.NewGuid(),
                RentalId = rentalId,
                Amount = dto.Amount,
                PaymentMethod = paymentMethodString,
                PaymentDate = DateTime.UtcNow
            };

            await _paymentRepository.AddPaymentAsync(payment);
            await _paymentRepository.SaveChangesAsync();

            return new PaymentResponseDto
            {
                Id = payment.Id,
                RentalId = payment.RentalId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                PaymentDate = payment.PaymentDate
            };
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetAllPaymentsAsync(
        Guid? rentalId = null,
        string? method = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
        {
            var payments = await _paymentRepository.GetAllPaymentsAsync(rentalId, method, startDate, endDate)
                           ?? Enumerable.Empty<TbPayment>();

            return payments.Select(pay => new PaymentResponseDto
            {
                Id = pay.Id,
                RentalId = pay.RentalId,
                Amount = pay.Amount,
                PaymentMethod = pay.PaymentMethod,
                PaymentDate = pay.PaymentDate
            });
        }
    }
}