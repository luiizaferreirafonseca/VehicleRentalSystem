using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Enums;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Resources;
using VehicleRentalSystem.Services.interfaces;

namespace VehicleRentalSystem.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _repository;

        public VehicleService(IVehicleRepository repository)
        {
            _repository = repository;
        }

        public async Task<VehicleResponseDTO> CreateVehicleAsync(VehicleCreateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Brand))
                throw new InvalidOperationException(Messages.VehicleBrandRequired);

            if (string.IsNullOrWhiteSpace(dto.Model))
                throw new InvalidOperationException(Messages.VehicleModelRequired);

            if (dto.Year <= 0)
                throw new InvalidOperationException(Messages.VehicleYearInvalid);

            if (dto.DailyRate <= 0)
                throw new InvalidOperationException(Messages.VehicleDailyRateInvalid);

            if (string.IsNullOrWhiteSpace(dto.LicensePlate))
                throw new InvalidOperationException(Messages.VehicleLicensePlateRequired);

            var plate = dto.LicensePlate.Trim();
            var plateExists = await _repository.ExistsByLicensePlateAsync(plate);

            if (plateExists)
                throw new InvalidOperationException(Messages.VehicleLicensePlateAlreadyExists);

            var vehicle = new TbVehicle
            {
                Id = Guid.NewGuid(), 
                Brand = dto.Brand.Trim(),
                Model = dto.Model.Trim(),
                Year = dto.Year,
                DailyRate = dto.DailyRate,
                LicensePlate = plate,
                Status = VehicleStatus.available.ToString()
            };

            var created = await _repository.CreateVehicleAsync(vehicle);

            return new VehicleResponseDTO
            {
                Id = created.Id,
                Brand = created.Brand,
                Model = created.Model,
                Year = created.Year,
                DailyRate = created.DailyRate,
                Status = created.Status,
                LicensePlate = created.LicensePlate
            };
        }

        public async Task RemoveVehicleAsync(Guid vehicleId)
        {
            if (vehicleId == Guid.Empty)
                throw new InvalidOperationException(Messages.VehicleIdRequired);

            var vehicle = await _repository.GetVehicleByIdAsync(vehicleId);
            if (vehicle == null)
                throw new KeyNotFoundException(Messages.VehicleNotFound);

            if (vehicle.Status == VehicleStatus.rented.ToString())
                throw new InvalidOperationException(Messages.VehicleCannotBeDeletedWhenRented);

            var deleted = await _repository.DeleteVehicleAsync(vehicle);

            if (!deleted)
                throw new InvalidOperationException(Messages.VehicleDeleteFailed);
        }
    }
}
