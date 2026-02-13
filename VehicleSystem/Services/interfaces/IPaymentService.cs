using VehicleRentalSystem.DTO;

namespace VehicleRentalSystem.Services.interfaces
{
    public interface IPaymentService
    {
        Task<RentalResponseDTO> RegisterPaymentAsync(Guid rentalId, PaymentCreateDTO dto);

    }
}