using VehicleRentalSystem.DTO;

namespace VehicleRentalSystem.Services
{
    public interface IPaymentService
    {
        Task<RentalResponseDTO> RegisterPaymentAsync(Guid rentalId, PaymentCreateDTO dto);

    }
}