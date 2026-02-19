using VehicleRentalSystem.DTO;
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
        public async Task<List<RentalResponseDTO>> GetRentalsAsync()
        {
            var rentalsFromDb = await _repository.GetRentalsAsync();

            var result = rentalsFromDb.Select(rental => new RentalResponseDTO
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

            return result;
        }

        public async Task<RentalResponseDTO> GetRentalByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("The rental identifier is required.");

            var rental = await _repository.GetRentalByIdAsync(id);

            if (rental == null)
                throw new KeyNotFoundException("Rental not found.");

            var result = new RentalResponseDTO
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

            return result;
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
                throw new InvalidOperationException("The rental ID is required.");

            var rental = await _repository.GetRentalByIdAsync(id);

            if (rental == null)
                throw new KeyNotFoundException("Rental not found.");

            if (rental.Status != RentalStatus.active.ToString())
                throw new InvalidOperationException($"It is not possible to cancel a rental with status '{rental.Status}'.");

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
                throw new KeyNotFoundException("Rental not found.");

            if (rental.Status != RentalStatus.active.ToString())
                throw new InvalidOperationException($"Only rentals with status 'active' can be updated. Current status: {rental.Status}");

            if (updateDto.NewExpectedEndDate <= rental.StartDate)
                throw new InvalidOperationException("The new return date must be later than the start date.");

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

            if (rental.Vehicle != null)
            {
                rental.Vehicle.Status = VehicleStatus.available.ToString();
            }
            else
            {
                await _repository.UpdateVehicleStatusAsync(
                    rental.VehicleId,
                    VehicleStatus.available.ToString()
                );
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
        public async Task<List<RentalResponseDTO>> SearchRentalsByUserAsync(Guid userId, string? status, int page)
        {
            if (userId == Guid.Empty)
                throw new InvalidOperationException(Messages.UserIdRequired);

            if (page < 1)
                throw new InvalidOperationException(Messages.PageInvalid);

            if (!string.IsNullOrEmpty(status) &&
            status != RentalStatus.active.ToString() &&
            status != RentalStatus.completed.ToString() &&
            status != RentalStatus.canceled.ToString())
            {
                throw new InvalidOperationException(Messages.InvalidStatus);
            }

            var rentals = await _repository.SearchRentalsByUserAsync(userId, status, page);

            return rentals.Select(r => new RentalResponseDTO
            {
                Id = r.Id,
                StartDate = r.StartDate,
                ExpectedEndDate = r.ExpectedEndDate,
                ActualEndDate = r.ActualEndDate,
                TotalAmount = r.TotalAmount,
                PenaltyFee = r.PenaltyFee,
                Status = r.Status,
                VehicleId = r.VehicleId,
                UserId = r.UserId,
                DailyRate = r.DailyRate,
                UserName = r.User?.Name ?? "",
                VehicleModel = r.Vehicle?.Model ?? ""
            }).ToList();
        }
    }
}

