using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Repositories.interfaces;

namespace VehicleRentalSystem.Services
{
    public class RentalService : IRentalService
    {
        private IRentalRepository _repository;

        public RentalService(IRentalRepository repository)
        {
            _repository = repository;
        }

        public List<RentalResponseDTO> GetRentals()
        {

        }

        public RentalResponseDTO? GetRentalById(Guid id)
        {
        }

    }
}