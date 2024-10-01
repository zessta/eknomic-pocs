using InventoryManagement.Enums;
using InventoryManagement.Models.Entities;

namespace InventoryManagement.Repositories.Interfaces
{
    public interface ITransitRepository
    {
        public Task<WarehouseInventory> GetInventoryFromWarehouse(int inventoryId, int warehouseId);
        public Task<bool> UpdateWarehouseStocks(WarehouseInventory inventory);
        public Task<bool> AddWarehouseStocks(WarehouseInventory inventory);
        public Task<EventStore> RaiseEvent<TEvent>(InventoryEvents eventType, TEvent raisedEvent);
    }
}
