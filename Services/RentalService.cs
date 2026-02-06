using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Repositories.interfaces;

namespace VehicleRentalSystem.Services
{
    public class RentalService : IRentalService
    {
        private readonly IRentalRepository _repository;

        public RentalService(IRentalRepository repository)
        {
            _repository = repository;
        }

        public List<RentalResponseDTO> GetRentals()
        {
            var rentalsFromDb = _repository.SelectRentals();

            return rentalsFromDb.Select(rental => new RentalResponseDTO
            {
                Id = rental.Id,
                StartDate = rental.StartDate,
                ExpectedEndDate = rental.ExpectedEndDate,
                ActualEndDate = rental.ActualEndDate,
                TotalAmount = rental.TotalAmount,
                PenaltyFee = rental.PenaltyFee,
                Status = rental.Status,
                VehicleId = rental.VehicleId,
                UserId = rental.UserId,
                DailyRate = rental.DailyRate,

                // relacionamentos com os includes para trazer as informacoes
                UserName = rental.User?.Name ?? "",
                VehicleModel = rental.Vehicle?.Model ?? ""
            }).ToList();
        }

        public RentalResponseDTO? GetRentalById(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            var rental = _repository.SelectRentalById(id);

            if (rental == null)
                return null;

            return new RentalResponseDTO
            {
                Id = rental.Id,
                StartDate = rental.StartDate,
                ExpectedEndDate = rental.ExpectedEndDate,
                ActualEndDate = rental.ActualEndDate,
                TotalAmount = rental.TotalAmount,
                PenaltyFee = rental.PenaltyFee,
                Status = rental.Status,
                VehicleId = rental.VehicleId,
                UserId = rental.UserId,
                DailyRate = rental.DailyRate,
                UserName = rental.User?.Name ?? "",
                VehicleModel = rental.Vehicle?.Model ?? ""
            };
        }


    }

}
