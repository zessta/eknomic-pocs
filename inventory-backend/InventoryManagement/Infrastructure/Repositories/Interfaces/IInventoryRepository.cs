using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.DTO;

namespace InventoryManagement.Infrastructure.Repositories.Interfaces
{
    public interface IInventoryRepository
    {
        Task<List<InventoryDto>> GetAllInventoryItemsAsync();
        Task<InventoryItem> AddInventoryItemAsync(InventoryDto itemDto, int warehouseId, int quantity);
        Task<InventoryItem> UpdateInventoryItemAsync(int id, InventoryDto itemDto);
        Task<bool> DeleteInventoryItemAsync(int id);
        Task<bool> UpdateInventoryQuantityAsync(int warehouseId, int inventoryItemId, int quantity);
        Task<List<WarehouseInventoryDto>> GetInventoryByWarehouseAsync(int warehouseId);
        Task<List<TotalInventoryDto>> GetTotalInventoryAsync();
    }
}
