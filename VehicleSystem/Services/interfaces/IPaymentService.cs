using VehicleRentalSystem.DTO;

namespace VehicleRentalSystem.Services.interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> RegisterPaymentAsync(Guid rentalId, PaymentCreateDTO dto);
        Task<IEnumerable<PaymentResponseDto>> GetAllPaymentsAsync(Guid? rentalId, string? method, DateTime? startDate, DateTime? endDate);
    }
}