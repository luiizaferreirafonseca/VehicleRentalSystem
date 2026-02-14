using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleRentalSystem.DTO;
using VehicleRentalSystem.Enums.VehicleRentalSystem.Enums;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;
using VehicleRentalSystem.Resources;
using VehicleRentalSystem.Services.interfaces;

namespace VehicleRentalSystem.Services
{
    public class AccessoryService : IAccessoryService
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IAccessoryRepository _accessoryRepository;

        public async Task<IEnumerable<AccessoryResponseDto>> GetAccessoriesAsync()
        {
            var accessories = await _accessoryRepository.GetAllAsync() ?? Enumerable.Empty<TbAccessory>();

            return accessories.Select(a => new AccessoryResponseDto
            {
                Id = a.Id,
                Name = a.Name,
                DailyRate = a.DailyRate
            });
        }

        public async Task<AccessoryResponseDto> GetAccessoryByIdAsync(Guid id)
        {
            var accessory = await _accessoryRepository.GetByIdAsync(id);
            if (accessory == null)
                throw new KeyNotFoundException("Acessório não encontrado.");

            return new AccessoryResponseDto
            {
                Id = accessory.Id,
                Name = accessory.Name,
                DailyRate = accessory.DailyRate
            };
        }

        public async Task<IEnumerable<AccessoryResponseDto>> GetAccessoriesByRentalIdAsync(Guid rentalId)
        {
            var rentalExists = await _rentalRepository.GetRentalByIdAsync(rentalId);
            if (rentalExists == null)
                throw new KeyNotFoundException("Locação não encontrada.");

            var accessories = await _accessoryRepository.GetByRentalIdAsync(rentalId) ?? Enumerable.Empty<TbAccessory>();

            return accessories.Select(a => new AccessoryResponseDto
            {
                Id = a.Id,
                Name = a.Name,
                DailyRate = a.DailyRate
            });
        }

        public AccessoryService(IRentalRepository rentalRepository, IAccessoryRepository accessoryRepository)
        {
            _rentalRepository = rentalRepository;
            _accessoryRepository = accessoryRepository;
        }

        public async Task<AccessoryResponseDto> CreateAccessoryAsync(AccessoryCreateDto dto)
        {
            var existing = await _accessoryRepository.GetByNameAsync(dto.Name);
            if (existing != null)
                throw new InvalidOperationException("Já existe um acessório cadastrado com este nome.");

            var accessory = new TbAccessory
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                DailyRate = dto.DailyRate 
            };

            await _accessoryRepository.AddAsync(accessory);

            return new AccessoryResponseDto
            {
                Id = accessory.Id,
                Name = accessory.Name,
                DailyRate = accessory.DailyRate
            };
        }

        public async Task AddAccessoryToRentalAsync(Guid rentalId, Guid accessoryId)
        {
            if (rentalId == Guid.Empty || accessoryId == Guid.Empty)
                throw new ArgumentException("Os identificadores de locação e acessório são obrigatórios.");

            var rental = await _rentalRepository.GetRentalByIdAsync(rentalId);
            if (rental == null)
                throw new KeyNotFoundException("Locação não encontrada.");

            if (rental.Status == RentalStatus.canceled.ToString())
                throw new InvalidOperationException("Não é possível atribuir acessórios a uma locação cancelada.");

            var accessory = await _accessoryRepository.GetByIdAsync(accessoryId);
            if (accessory == null)
                throw new KeyNotFoundException("Acessório não encontrado.");

            var alreadyLinked = await _accessoryRepository.IsLinkedToRentalAsync(rentalId, accessoryId);
            if (alreadyLinked)
                throw new InvalidOperationException("Este acessório já está vinculado a esta locação.");

                 await _accessoryRepository.LinkToRentalAsync(rentalId, accessoryId);

            var days = (rental.ExpectedEndDate.Date - rental.StartDate.Date).Days;
            if (days <= 0) days = 1;

              decimal accessoryTotalValue = accessory.DailyRate * days;

               rental.TotalAmount += accessoryTotalValue;

            await _rentalRepository.UpdateAsync(rental);

        }


        public async Task RemoveAccessoryFromRentalAsync(Guid rentalId, Guid accessoryId)
        {
            var rental = await _rentalRepository.GetRentalByIdAsync(rentalId);
            if (rental == null) throw new KeyNotFoundException("Locação não encontrada.");

            var accessory = await _accessoryRepository.GetByIdAsync(accessoryId);
            if (accessory == null) throw new KeyNotFoundException("Acessório não encontrado.");

            var isLinked = await _accessoryRepository.IsLinkedToRentalAsync(rentalId, accessoryId);
            if (!isLinked)
                throw new KeyNotFoundException("Este acessório não está vinculado a esta locação.");

            await _accessoryRepository.RemoveLinkAsync(rentalId, accessoryId);

            var days = (rental.ExpectedEndDate.Date - rental.StartDate.Date).Days;
            if (days <= 0) days = 1;

            rental.TotalAmount -= (accessory.DailyRate * days);

            await _rentalRepository.UpdateAsync(rental);
        }
    }
}