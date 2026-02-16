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
            // Busca a entidade completa com os Includes (Joins) no repositório
            var rental = await _reportRepository.GetRentalWithDetailsAsync(id);

            // Se não encontrar, retorna null (o Controller tratará como 404)
            if (rental == null)
            {
                return null;
            }

            // Utiliza o Mapper (Extension Method) para converter para o DTO de relatório
            return rental.ToReportDto();
        }
    }
}