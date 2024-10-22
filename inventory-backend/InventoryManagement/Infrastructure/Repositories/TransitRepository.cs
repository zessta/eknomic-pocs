using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Interfaces;
using InventoryManagement.Domain.DTO;

namespace InventoryManagement.Infrastructure.Repositories
{
    public class TransitRepository : ITransitRepository
    {
        private readonly InventoryDbContext _context;
        public TransitRepository(InventoryDbContext context)
        {
            _context = context;
        }
        public async Task<WarehouseInventoryDto> GetInventoryFromWarehouse(string inventoryId, string warehouseId)
        {
            var warehouseInventory = await _context.WarehouseInventories.AsNoTracking().FirstOrDefaultAsync(iv => iv.InventoryItemId == int.Parse(inventoryId) && iv.WarehouseId == int.Parse(warehouseId));
            return new WarehouseInventoryDto
            {
                WarehouseInventoryId = warehouseInventory.WarehouseInventoryId.ToString(),
                WarehouseId = warehouseInventory.WarehouseId.ToString(),
                InventoryItemId = warehouseInventory.InventoryItemId.ToString(),
                Quantity = warehouseInventory.Quantity
            };
        }

        public async Task<bool> UpdateWarehouseStocks(WarehouseInventoryDto inventory)
        {
            try
            {
                var updatedWarehouseInventory = new WarehouseInventory
                {
                    WarehouseInventoryId = int.Parse(inventory.WarehouseInventoryId),
                    WarehouseId = int.Parse(inventory.WarehouseId),
                    InventoryItemId = int.Parse(inventory.InventoryItemId),
                    Quantity = inventory.Quantity
                };

                _context.WarehouseInventories.Attach(updatedWarehouseInventory);
                _context.Entry(updatedWarehouseInventory).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddWarehouseStocks(WarehouseInventoryDto inventory)
        {
            try
            {
                _context.WarehouseInventories.Add(new WarehouseInventory
                {
                    WarehouseId = int.Parse(inventory.WarehouseId),
                    InventoryItemId = int.Parse(inventory.InventoryItemId),
                    Quantity = inventory.Quantity
                });
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
