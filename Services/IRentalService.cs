using VehicleRentalSystem.DTO;

namespace VehicleRentalSystem.Services
{
    public interface IRentalService 
    {
        List<RentalResponseDTO> GetRentals();
        RentalResponseDTO? GetRentalById(Guid id);
    }
}
