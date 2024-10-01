using InventoryManagement.Models.DTO;
using InventoryManagement.Models.Entities;

namespace InventoryManagement.Services.Interfaces
{
    public interface IInventoryService
    {
        public Task<List<InventoryDto>> GetAllSKU();
        public Task<InventoryItem> AddInventoryItemAsync(InventoryDto item, int warehouseId, int quantity);
        public Task<InventoryItem> UpdateInventoryItemAsync(int id, InventoryDto itemDto);
        public Task<bool> DeleteInventoryItemAsync(int id);
        public Task<bool> UpdateInventoryQuantityAsync(int warehouseId, int inventoryItemId, int quantity);
        public Task<List<WarehouseInventoryDto>> GetInventoryByWarehouseAsync(int warehouseId);
        public Task<List<TotalInventoryDto>> GetTotalInventoryAsync();
    }
}
