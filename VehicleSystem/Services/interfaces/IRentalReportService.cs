using VehicleRentalSystem.DTO;

namespace VehicleRentalSystem.Services.interfaces
{
    public interface IRentalReportService
    {
        Task<RentalReportResponseDTO?> GetRentalReportAsync(Guid id);
        Task<byte[]?> ExportRentalReportAsync(Guid id);
        Task<byte[]?> ExportRentalReportCsvAsync(Guid id);
        Task<string?> SaveRentalReportToRepositoryAsync(Guid id, string? format = "txt");
    }
}
