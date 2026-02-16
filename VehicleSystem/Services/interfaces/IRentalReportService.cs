using VehicleRentalSystem.DTO;

namespace VehicleRentalSystem.Services.interfaces
{
    public interface IRentalReportService
    {
        Task<RentalReportResponseDTO?> GetRentalReportAsync(Guid id);
    }
}
