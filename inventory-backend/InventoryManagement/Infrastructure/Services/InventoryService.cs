using InventoryManagement.Domain.DTO;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Infrastructure.Repositories.Interfaces;
using InventoryManagement.Infrastructure.Services.Interfaces;

namespace InventoryManagement.Infrastructure.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IEventRepository _eventRepository;
        public InventoryService(IInventoryRepository inventoryRepository,IEventRepository eventRepository)
        {
            _inventoryRepository = inventoryRepository;
            _eventRepository = eventRepository;
        }
        public async Task<List<InventoryDto>> GetAllSKU()
        {
            var inventories = await _inventoryRepository.GetAllInventoryItemsAsync();
            return inventories;
        }

        public async Task<string> AddInventoryItemAsync(InventoryDto item, string warehouseId, int quantity)
        {
            var inventoryId = await _inventoryRepository.AddInventoryItemAsync(item, warehouseId, quantity);
            await _eventRepository.RaiseEvent(InventoryEvents.NewStockAdded, inventoryId, warehouseId);
            return inventoryId;
        }

        public async Task<bool> UpdateInventoryItemAsync(string id, InventoryDto itemDto)
        {
            return await _inventoryRepository.UpdateInventoryItemAsync(id, itemDto);
        }

        public async Task<bool> DeleteInventoryItemAsync(string id)
        {
            return await _inventoryRepository.DeleteInventoryItemAsync(id);
        }

        public async Task<bool> UpdateInventoryQuantityAsync(string warehouseId, string inventoryItemId, int quantity)
        {
            var status = await _inventoryRepository.UpdateInventoryQuantityAsync(warehouseId, inventoryItemId, quantity);
            await _eventRepository.RaiseEvent(InventoryEvents.StockAdded, quantity, warehouseId);
            return status;
        }

        public async Task<List<WarehouseStockDto>> GetInventoryByWarehouseAsync(string warehouseId)
        {
            return await _inventoryRepository.GetInventoryByWarehouseAsync(warehouseId);
        }

        public async Task<List<TotalInventoryDto>> GetTotalInventoryAsync()
        {
            return await _inventoryRepository.GetTotalInventoryAsync();
        }
    }
}
