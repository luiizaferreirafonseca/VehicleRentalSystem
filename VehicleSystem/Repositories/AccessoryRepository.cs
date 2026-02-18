using Microsoft.EntityFrameworkCore;
using VehicleRentalSystem.Models;
using VehicleRentalSystem.Repositories.interfaces;

namespace VehicleRentalSystem.Repositories;

public class AccessoryRepository : IAccessoryRepository
{
    private readonly PostgresContext _context;

    public AccessoryRepository(PostgresContext context)
    {
        _context = context;
    }

    public async Task<TbAccessory?> GetByIdAsync(Guid id)
    {
        return await _context.TbAccessories.FindAsync(id);
    }

    public async Task<TbAccessory?> GetByNameAsync(string name)
    {
        return await _context.TbAccessories.FirstOrDefaultAsync(a => a.Name == name);
    }

    public async Task<IEnumerable<TbAccessory>?> GetAllAsync()
    {
        return await _context.TbAccessories.ToListAsync();
    }

    public async Task<IEnumerable<TbAccessory>?> GetByRentalIdAsync(Guid rentalId)
    {
        // join tb_rental_accessory
        return await _context.TbRentalAccessories
            .Where(ra => ra.RentalId == rentalId)
            .Include(ra => ra.Accessory)
            .Select(ra => ra.Accessory)
            .ToListAsync();
    }

    public async Task AddAsync(TbAccessory accessory)
    {
        await _context.TbAccessories.AddAsync(accessory);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsLinkedToRentalAsync(Guid rentalId, Guid accessoryId)
    {
        return await _context.TbRentalAccessories.AnyAsync(ra => ra.RentalId == rentalId && ra.AccessoryId == accessoryId);
    }

    public async Task LinkToRentalAsync(Guid rentalId, Guid accessoryId)
    {
        var ra = new TbRentalAccessory
        {
            RentalId = rentalId,
            AccessoryId = accessoryId,
            Quantity = 1,
            UnitPrice = (await GetByIdAsync(accessoryId))?.DailyRate ?? 0m,
            TotalPrice = (await GetByIdAsync(accessoryId))?.DailyRate ?? 0m
        };

        await _context.TbRentalAccessories.AddAsync(ra);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveLinkAsync(Guid rentalId, Guid accessoryId)
    {
        var ra = await _context.TbRentalAccessories.FirstOrDefaultAsync(x => x.RentalId == rentalId && x.AccessoryId == accessoryId);
        if (ra != null)
        {
            _context.TbRentalAccessories.Remove(ra);
            await _context.SaveChangesAsync();
        }
    }
}
