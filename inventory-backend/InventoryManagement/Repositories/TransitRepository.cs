using InventoryManagement.Data;
using InventoryManagement.Enums;
using InventoryManagement.Models.Entities;
using InventoryManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace InventoryManagement.Repositories
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

        public async Task<EventStore> RaiseEvent<TEvent>(InventoryEvents eventType, TEvent raisedEvent)
        {
            var eventDetails = new EventStore()
            {
                EventType = eventType,
                EventData = JsonSerializer.Serialize(raisedEvent),
                TriggeredBy = Guid.NewGuid().ToString()
            };

            await _context.EventStore.AddAsync(eventDetails);
            await _context.SaveChangesAsync();
            return eventDetails;
        }
    }
}
