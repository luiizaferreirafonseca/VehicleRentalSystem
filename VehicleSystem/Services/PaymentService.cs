using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Enums;
using VehicleRentalSystem.Enums.VehicleRentalSystem.Enums;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Resources;

namespace VehicleRentalSystem.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRentalRepository _repository;

        public PaymentService(IRentalRepository repository)
        {
            _repository = repository;
        }

        public async Task<RentalResponseDTO> RegisterPaymentAsync(Guid rentalId, PaymentCreateDTO dto)
        {
            if (rentalId == Guid.Empty)
                throw new ArgumentException("O identificador da locação é obrigatório.");

            var rental = await _repository.GetRentalByIdAsync(rentalId);

            if (rental == null)
                throw new KeyNotFoundException(Messages.RentalNotFound);

            if (rental.Status == RentalStatus.canceled.ToString())
                throw new InvalidOperationException("Não é possível registrar pagamento em uma locação cancelada.");

            if (dto.Amount <= 0)
                throw new InvalidOperationException("O valor do pagamento deve ser maior que zero.");

            var totalPayments = await _repository.GetTotalPaymentsAsync(rentalId);
            var remaining = (rental.TotalAmount ?? 0m) - totalPayments;

            if (dto.Amount > remaining)
                throw new InvalidOperationException("A soma dos pagamentos não pode exceder o valor total da locação.");

            var paymentMethodString = dto.PaymentMethod.HasValue ? dto.PaymentMethod.Value.ToString() : string.Empty;

            var payment = new TbPayment
            {
                Id = Guid.NewGuid(),
                RentalId = rentalId,
                Amount = dto.Amount,
                PaymentMethod = paymentMethodString,
                PaymentDate = DateTime.UtcNow
            };

            await _repository.AddPaymentAsync(payment);
            await _repository.SaveChangesAsync();

            rental = await _repository.GetRentalByIdAsync(rentalId);

            return new RentalResponseDTO
            {
                Id = rental.Id,
                StartDate = rental.StartDate,
                ExpectedEndDate = rental.ExpectedEndDate,
                ActualEndDate = rental.ActualEndDate,
                TotalAmount = rental.TotalAmount,
                PenaltyFee = rental.PenaltyFee,
                Status = rental.Status,
                VehicleId = rental.VehicleId,
                UserId = rental.UserId,
                DailyRate = rental.DailyRate,
                UserName = rental.User?.Name ?? "",
                VehicleModel = rental.Vehicle?.Model ?? ""
            };
        }
    }
}