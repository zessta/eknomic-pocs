using InventoryManagement.Domain.DTO;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly InventoryDbContext _context;

        public WarehouseRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync()
        {
            return await _context.Warehouses.Select(x => new WarehouseDto
            {
              WarehouseId = x.WarehouseId.ToString(),
              Location = x.Location,
              ManagerId = x.ManagerId
            }).ToListAsync();
        }

        public async Task<WarehouseDto> GetWarehouseByIdAsync(string id)
        {
            var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.WarehouseId == int.Parse(id));
            return new WarehouseDto
            {
                WarehouseId = warehouse.WarehouseId.ToString(),
                Location = warehouse.Location,
                ManagerId = warehouse.ManagerId
            };
        }

        public async Task AddWarehouseAsync(WarehouseDto warehouse)
        {
            await _context.Warehouses.AddAsync(new Warehouse() { Location = warehouse.Location, ManagerId = warehouse.ManagerId});
            await _context.SaveChangesAsync();
        }

        public async Task DeleteWarehouseAsync(string id)
        {
            var warehouse = await _context.Warehouses.FindAsync(int.Parse(id));
            if (warehouse != null)
            {
                _context.Warehouses.Remove(warehouse);
                await _context.SaveChangesAsync();
            }
        }
    }
}
