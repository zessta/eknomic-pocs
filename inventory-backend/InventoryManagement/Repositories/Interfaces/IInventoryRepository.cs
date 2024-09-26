using InventoryManagement.Models.Entities;
using InventoryManagement.Models;

namespace InventoryManagement.Repositories.Interfaces
{
    public interface IInventoryRepository
    {
        Task<List<InventoryItemDto>> GetAllInventoryItemsAsync();
        Task<InventoryItem> AddInventoryItemAsync(InventoryItemDto itemDto, int warehouseId, int quantity);
        Task<InventoryItem> UpdateInventoryItemAsync(int id, InventoryItemDto itemDto);
        Task<bool> DeleteInventoryItemAsync(int id);
        Task<bool> UpdateInventoryQuantityAsync(int warehouseId, int inventoryItemId, int quantity);
        Task<List<WarehouseInventoryDto>> GetInventoryByWarehouseAsync(int warehouseId);
        Task<List<TotalInventoryDto>> GetTotalInventoryAsync();
    }
}
