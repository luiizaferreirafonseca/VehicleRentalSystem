using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleRentalSystem.DTO;

namespace VehicleRentalSystem.Services.interfaces;

public interface IAccessoryService
{
    Task<AccessoryResponseDto> GetAccessoryByIdAsync(Guid id);
    Task<IEnumerable<AccessoryResponseDto>> GetAccessoriesAsync();
    Task<IEnumerable<AccessoryResponseDto>> GetAccessoriesByRentalIdAsync(Guid rentalId);
    Task<AccessoryResponseDto> CreateAccessoryAsync(AccessoryCreateDto dto);
    Task AddAccessoryToRentalAsync(Guid rentalId, Guid accessoryId);
    Task RemoveAccessoryFromRentalAsync(Guid rentalId, Guid accessoryId);
}