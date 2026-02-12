using System.Threading.Tasks;
using VehicleRentalSystem.DTO;

namespace VehicleRentalSystem.Services
{
    public interface IRentalService 
    {
        List<RentalResponseDTO> GetRentals();
        RentalResponseDTO? GetRentalById(Guid id);
        Task<RentalResponseDTO> CreateRentalAsync(RentalCreateDTO dto);
        Task<RentalResponseDTO> CancelRentalAsync(Guid id);
        Task<RentalResponseDTO> UpdateRentalDatesAsync(Guid id, UpdateRentalDTO updateDto);
        Task<RentalResponseDTO> ReturnRentalAsync(Guid id);
        Task<RentalResponseDTO> RegisterPaymentAsync(Guid rentalId, DTO.PaymentCreateDTO dto);

    }
}
