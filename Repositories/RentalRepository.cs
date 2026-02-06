using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;

namespace VehicleRentalSystem.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly PostgresContext _postgresContext;

        public RentalRepository(PostgresContext postgresContext)
        {
            _postgresContext = postgresContext;
        }

        public List<TbRental> SelectRentals()
        {
            return _postgresContext.TbRentals
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .ToList();
        }


        public TbRental? SelectRentalById(Guid id)
        {
            return _postgresContext.TbRentals
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .FirstOrDefault(r => r.Id == id);
        }

    }
}
