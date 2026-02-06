using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories.interfaces
{
    public interface IRentalRepository
    {
        List<TbRental> SelectRentals();

        TbRental SelectRentalById(Guid id);
    }
}
