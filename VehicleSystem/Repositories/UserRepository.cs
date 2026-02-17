using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;

namespace VehicleRentalSystem.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly PostgresContext _postgresContext;

        public UserRepository(PostgresContext postgresContext)
        {
            _postgresContext = postgresContext;
        }

        public async Task<List<TbUser>> GetAllUsersAsync()
        {
            return await _postgresContext.TbUsers
                .Include(u => u.TbRentals)
                .ToListAsync();
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _postgresContext.TbUsers.AnyAsync(u => u.Email == email);
        }

        public async Task<TbUser> CreateUserAsync(TbUser user)
        {
            await _postgresContext.TbUsers.AddAsync(user);
            await _postgresContext.SaveChangesAsync();
            return user;
        }
    }
}
