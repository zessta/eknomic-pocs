using InventoryManagement.Domain.DTO;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Raven.Entities;
using InventoryManagement.Infrastructure.Data.Raven;
using InventoryManagement.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.Raven
{
    public class TransitRepository : ITransitRepository
    {
        private readonly RavenDbContext _context;
        public TransitRepository(RavenDbContext context)
        {
            _context = context;
        }
        public async Task<bool> AddWarehouseStocks(WarehouseInventoryDto inventory)
        {
            using var session = _context.AsyncSession;
            var newWarehouseInventory = new WarehouseInventory
            {
                WarehouseId = inventory.WarehouseId,
                InventoryItemId = inventory.InventoryItemId,
                Quantity = inventory.Quantity
            };

            await session.StoreAsync(newWarehouseInventory);
            await session.SaveChangesAsync();
            return true;
        }

        public async Task<WarehouseInventoryDto> GetInventoryFromWarehouse(string inventoryId, string warehouseId)
        {
            using var session = _context.AsyncSession;
            var warehouseInventory = await session.Query<WarehouseInventory>()
            .FirstOrDefaultAsync(iv => iv.InventoryItemId == inventoryId && iv.WarehouseId == warehouseId);

            if (warehouseInventory == null)
                return null;

            return new WarehouseInventoryDto
            {
                WarehouseInventoryId = warehouseInventory.Id,
                WarehouseId = warehouseInventory.WarehouseId,
                InventoryItemId = warehouseInventory.InventoryItemId,
                Quantity = warehouseInventory.Quantity
            };
        }

        public async Task<EventStoreDto> RaiseEvent<TEvent>(InventoryEvents eventType, TEvent eventData, string warehouseId)
        {
            using var session = _context.AsyncSession;
            var eventDetails = new EventStore
            {
                EventType = eventType,
                EventData = eventData?.ToString(),
                WarehouseId = warehouseId
            };

            await session.StoreAsync(eventDetails);
            await session.SaveChangesAsync();

            return new EventStoreDto
            {
                EventId = eventDetails.EventId,
                EventType = eventType,
                EventData = eventData?.ToString(),
                WarehouseId = warehouseId
            };
        }

        public async Task<bool> UpdateWarehouseStocks(WarehouseInventoryDto inventory)
        {
            using var session = _context.AsyncSession;
            var warehouseInventory = await session.LoadAsync<WarehouseInventory>(inventory.WarehouseInventoryId);

            if (warehouseInventory == null)
                return false;

            warehouseInventory.WarehouseId = inventory.WarehouseId;
            warehouseInventory.InventoryItemId = inventory.InventoryItemId;
            warehouseInventory.Quantity = inventory.Quantity;

            await session.SaveChangesAsync();
            return true;
        }
    }
}
