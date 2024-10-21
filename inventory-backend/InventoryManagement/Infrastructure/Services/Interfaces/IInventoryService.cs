using InventoryManagement.Domain.DTO;
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Infrastructure.Services.Interfaces
{
    public interface IInventoryService
    {
        public Task<List<InventoryDto>> GetAllSKU();
        public Task<string> AddInventoryItemAsync(InventoryDto item, string warehouseId, int quantity);
        public Task<bool> UpdateInventoryItemAsync(string id, InventoryDto itemDto);
        public Task<bool> DeleteInventoryItemAsync(string id);
        public Task<bool> UpdateInventoryQuantityAsync(string warehouseId, string inventoryItemId, int quantity);
        public Task<List<WarehouseStockDto>> GetInventoryByWarehouseAsync(string warehouseId);
        public Task<List<TotalInventoryDto>> GetTotalInventoryAsync();
    }
}
