using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;

namespace VehicleRentalSystem.Repositories
{
    public class RentalReportRepository : IRentalReportRepository
    {
        private readonly PostgresContext _context;

        public RentalReportRepository(PostgresContext context)
        {
            _context = context;
        }

        public async Task<TbRental?> GetRentalWithDetailsAsync(Guid rentalId)
        {//
            // Realiza os Joins necessários usando Include para carregar os dados relacionados
            return await _context.TbRentals
                .AsNoTracking()
                .Include(r => r.Vehicle)   // Join com a tabela de Veículos
                .Include(r => r.User)  // Join com a tabela de Usuários (cliente)
                .Include(r => r.TbPayments)  // Join com a tabela de Pagamentos
                .Include(r => r.TbRentalAccessories) // Join com a tabela de acessórios relacionados à locação
                    .ThenInclude(ra => ra.Accessory) // Inclui os detalhes do acessório
                .FirstOrDefaultAsync(r => r.Id == rentalId);
        }
    }
}