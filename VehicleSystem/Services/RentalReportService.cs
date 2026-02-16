using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Services.interfaces;
using VehicleRentalSystem.Services.Mappers;

namespace VehicleRentalSystem.Services
{
    public class RentalReportService : IRentalReportService
    {
        private readonly IRentalReportRepository _reportRepository;

        public RentalReportService(IRentalReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<RentalReportResponseDTO?> GetRentalReportAsync(Guid id)
        {
            var rental = await _reportRepository.GetRentalWithDetailsAsync(id);

            if (rental == null)
            {
                return null;
            }

            // Utiliza o Mapper (Extension Method) para converter para o DTO de relatório
            return rental.ToReportDto();
        }
    }
}