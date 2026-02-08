using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Enums.VehicleRentalSystem.Enums;
using VehicleRentalSystem.Enums;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Resources;

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

        public async Task<RentalResponseDTO> CreateRentalAsync(RentalCreateDTO dto) // No create, está apenas sendo enviado (pela função adm) dados obrigatorios, o resto como total amount etc estão sendo calculados e manipulados pela logica do backend.
        {
            
            if (dto.UserId == Guid.Empty)
                throw new InvalidOperationException(Messages.UserIdRequired);

            if (dto.VehicleId == Guid.Empty)
                throw new InvalidOperationException(Messages.VehicleIdRequired);


            if (dto.ExpectedEndDate == default)
                throw new InvalidOperationException(Messages.ExpectedEndDateRequired);

            if (dto.ExpectedEndDate <= dto.StartDate)
                throw new InvalidOperationException(Messages.ExpectedEndDateInvalid);

            var user = await _repository.GetUserById(dto.UserId);
            if (user == null)
                throw new KeyNotFoundException(Messages.UserNotFound);

            var vehicle = await _repository.GetVehicleById(dto.VehicleId);
            if (vehicle == null)
                throw new KeyNotFoundException(Messages.VehicleNotFound);

            var startDate = dto.StartDate ?? DateTime.UtcNow;

            var days = (dto.ExpectedEndDate.Date - startDate.Date).Days;

            if(days <= 0)
            {
                days = 1;
            }

            var totalAmount = vehicle.DailyRate * days;

            var rental = new TbRental
            {
                Id = Guid.NewGuid(),
                StartDate = startDate,
                ExpectedEndDate = dto.ExpectedEndDate,
                ActualEndDate = null,
                TotalAmount = totalAmount,
                PenaltyFee = 0m,
                Status = RentalStatus.active.ToString(),
                VehicleId = vehicle.Id,
                UserId = user.Id,
                DailyRate = vehicle.DailyRate
            };

            var createdRental = await _repository.CreateRentalAsync(rental);

            var rented = VehicleStatus.rented.ToString();
            var updated = await _repository.UpdateVehicleStatusAsync(vehicle.Id, rented);

            if (!updated)
                throw new InvalidOperationException(Messages.VehicleStatusUpdateFailed);

            return new RentalResponseDTO
            {
                Id = createdRental.Id,
                StartDate = createdRental.StartDate,
                ExpectedEndDate = createdRental.ExpectedEndDate,
                ActualEndDate = createdRental.ActualEndDate,
                TotalAmount = createdRental.TotalAmount,
                PenaltyFee = createdRental.PenaltyFee,
                Status = createdRental.Status,
                VehicleId = createdRental.VehicleId,
                UserId = createdRental.UserId,
                DailyRate = createdRental.DailyRate,
                UserName = user.Name,

                VehicleModel = vehicle.Model
            };
        }
    }
}

