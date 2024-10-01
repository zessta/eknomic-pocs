using InventoryManagement.Models.DTO;
using InventoryManagement.Models.Entities;
using InventoryManagement.Repositories.Interfaces;
using InventoryManagement.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        public InventoryService(IInventoryRepository inventoryRepository) 
        {
            _inventoryRepository = inventoryRepository;
        }
        public async Task<List<InventoryDto>> GetAllSKU()
        {
            var inventories = await _inventoryRepository.GetAllInventoryItemsAsync();
            return inventories;
        }

        public async Task<InventoryItem> AddInventoryItemAsync(InventoryDto item, int warehouseId, int quantity)
        {
            return await _inventoryRepository.AddInventoryItemAsync(item, warehouseId, quantity);
        }

        public async Task<InventoryItem> UpdateInventoryItemAsync(int id, InventoryDto itemDto)
        {
            return await _inventoryRepository.UpdateInventoryItemAsync(id, itemDto);
        }

        public async Task<bool> DeleteInventoryItemAsync(int id)
        {
            return await _inventoryRepository.DeleteInventoryItemAsync(id);
        }

        public async Task<bool> UpdateInventoryQuantityAsync(int warehouseId, int inventoryItemId, int quantity)
        {
            return await _inventoryRepository.UpdateInventoryQuantityAsync(warehouseId, inventoryItemId, quantity);
        }

        public async Task<List<WarehouseInventoryDto>> GetInventoryByWarehouseAsync(int warehouseId)
        {
            return await _inventoryRepository.GetInventoryByWarehouseAsync(warehouseId);
        }

        public async Task<List<TotalInventoryDto>> GetTotalInventoryAsync()
        {
            return await _inventoryRepository.GetTotalInventoryAsync();
        }
    }
}
