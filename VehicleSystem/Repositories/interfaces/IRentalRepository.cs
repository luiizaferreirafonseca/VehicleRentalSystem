// IRentalRepository.cs
using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories.interfaces
{
    public interface IRentalRepository
    {
        Task<List<TbRental>> GetRentalsAsync();
        Task<TbUser?> GetUserById(Guid id);
        Task<TbVehicle?> GetVehicleById(Guid id);

        Task<TbRental> CreateRentalAsync(TbRental rental);

        Task<bool> UpdateVehicleStatusAsync(Guid vehicleId, string status);
        Task<TbRental?> GetRentalByIdAsync(Guid id);
        Task UpdateAsync(TbRental rental);
        Task<List<TbRental>> SearchRentalsByUserAsync(Guid userId, string? status, int page);

        Task SaveChangesAsync();
    }
}
