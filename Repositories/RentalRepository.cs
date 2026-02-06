using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;

namespace VehicleRentalSystem.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private PostgresContext postgresContext;

        public RentalRepository(PostgresContext postgresContext)
        {
            postgresContext = postgresContext;
        }

        public List<TbRental> SelectRentals()
        {
            List<TbRental> tbRentals = postgresContext.TbRentals.ToList();
            return tbRentals;
        }

        public TbRental? SelectRentalById(Guid id)
        {
            return postgresContext.TbRentals.FirstOrDefault(r => r.Id == id);
        }

    }
}
