using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories.interfaces;

public interface IAccessoryRepository
{
    Task<TbAccessory?> GetByIdAsync(Guid id);
    Task<TbAccessory?> GetByNameAsync(string name);
    Task<IEnumerable<TbAccessory>> GetAllAsync();
    Task<IEnumerable<TbAccessory>> GetByRentalIdAsync(Guid rentalId);
    Task AddAsync(TbAccessory accessory);
    Task<bool> IsLinkedToRentalAsync(Guid rentalId, Guid accessoryId);
    Task LinkToRentalAsync(Guid rentalId, Guid accessoryId);
    Task RemoveLinkAsync(Guid rentalId, Guid accessoryId);
}