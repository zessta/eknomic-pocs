using InventoryManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Interfaces;

namespace InventoryManagement.Infrastructure.Repositories
{
    public class TransitRepository : ITransitRepository
    {
        private readonly InventoryDbContext _context;
        public TransitRepository(InventoryDbContext context)
        {
            _context = context;
        }
        public async Task<WarehouseInventory> GetInventoryFromWarehouse(int inventoryId, int warehouseId)
        {
            return await _context.WarehouseInventories.FirstOrDefaultAsync(iv => iv.InventoryItemId == inventoryId && iv.WarehouseId == warehouseId);
        }

        public async Task<bool> UpdateWarehouseStocks(WarehouseInventory inventory)
        {
            try
            {
                _context.WarehouseInventories.Update(inventory);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddWarehouseStocks(WarehouseInventory inventory)
        {
            try
            {
                _context.WarehouseInventories.Add(inventory);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<EventStore> RaiseEvent<TEvent>(InventoryEvents eventType, TEvent eventData, int warehouseId)
        {
            var eventDetails = new EventStore()
            {
                EventType = eventType,
                EventData = eventData?.ToString(),
                WarehouseId = warehouseId
            };

            await _context.EventStore.AddAsync(eventDetails);
            await _context.SaveChangesAsync();
            return eventDetails;
        }
    }
}
