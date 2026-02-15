using VehicleRentalSystem.Models;

namespace VehicleRentalSystem.Repositories.interfaces
{
    public interface IUserRepository
    {
        Task<List<TbUser>> GetAllUsersAsync();
    }
}
