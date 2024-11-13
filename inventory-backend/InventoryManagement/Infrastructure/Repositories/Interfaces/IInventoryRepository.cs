using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.DTO;

namespace InventoryManagement.Infrastructure.Repositories.Interfaces
{
    public interface IInventoryRepository
    {
        Task<List<InventoryDto>> GetAllInventoryItemsAsync();
        Task<string> AddInventoryItemAsync(InventoryDto itemDto, string warehouseId, int quantity);
        Task<bool> UpdateInventoryItemAsync(string id, InventoryDto itemDto);
        Task<bool> DeleteInventoryItemAsync(string id);
        Task<bool> UpdateInventoryQuantityAsync(string warehouseId, string inventoryItemId, int quantity);
        Task<List<WarehouseStockDto>> GetInventoryByWarehouseAsync(string warehouseId);
        Task<List<TotalInventoryDto>> GetTotalInventoryAsync();
    }
}
