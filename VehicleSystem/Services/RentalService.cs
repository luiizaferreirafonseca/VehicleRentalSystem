using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Enums.VehicleRentalSystem.Enums;
using VehicleRentalSystem.Enums;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Resources;
using VehicleRentalSystem.Services.interfaces;

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

        public async Task<RentalResponseDTO> CreateRentalAsync(RentalCreateDTO dto)
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

            if (days <= 0)
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

        public async Task<RentalResponseDTO> CancelRentalAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("O ID da locação é obrigatório.");

            var rental = await _repository.GetRentalByIdAsync(id);

            if (rental == null)
                throw new KeyNotFoundException("Locação não encontrada.");

            if (rental.Status != RentalStatus.active.ToString())
                throw new InvalidOperationException($"Não é possível cancelar uma locação com status '{rental.Status}'.");

            rental.Status = RentalStatus.canceled.ToString();

            if (rental.Vehicle != null)
            {
                rental.Vehicle.Status = VehicleStatus.available.ToString();
            }

            await _repository.SaveChangesAsync();

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

        public async Task<RentalResponseDTO> UpdateRentalDatesAsync(Guid id, UpdateRentalDTO updateDto)
        {
            var rental = await _repository.GetRentalByIdAsync(id);

            if (rental == null)
                throw new KeyNotFoundException("Locação não encontrada.");

            if (rental.Status != RentalStatus.active.ToString())
                throw new InvalidOperationException($"Só é permitido atualizar locações que estejam 'active'. Status atual: {rental.Status}");

            if (updateDto.NewExpectedEndDate <= rental.StartDate)
                throw new InvalidOperationException("A nova data de devolução deve ser posterior à data de início.");

            var days = (updateDto.NewExpectedEndDate.Date - rental.StartDate.Date).Days;

            if (days <= 0) days = 1;

            rental.ExpectedEndDate = updateDto.NewExpectedEndDate;
            rental.TotalAmount = days * rental.DailyRate;

            await _repository.UpdateAsync(rental);

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

        public async Task<RentalResponseDTO> ReturnRentalAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException(Messages.RentalIdRequired);

            var rental = await _repository.GetRentalByIdAsync(id);

            if (rental == null)
                throw new KeyNotFoundException(Messages.RentalNotFound);

            if (rental.Status != RentalStatus.active.ToString())
                throw new InvalidOperationException(Messages.RentalNotActive);

            var actualEndDate = DateTime.UtcNow;
            rental.ActualEndDate = actualEndDate;

            int penaltyDays = (actualEndDate.Date - rental.ExpectedEndDate.Date).Days;
            if (penaltyDays < 0)
                penaltyDays = 0;

            decimal penaltyFee = 0m;
            if (penaltyDays > 0)
                penaltyFee = rental.DailyRate * penaltyDays;

            rental.PenaltyFee = penaltyFee;

            rental.Status = RentalStatus.completed.ToString();

            rental.Vehicle.Status = VehicleStatus.available.ToString();

            await _repository.SaveChangesAsync();

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

