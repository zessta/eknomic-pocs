using InventoryManagement.Domain.DTO;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Infrastructure.Repositories.Interfaces
{
    public interface ITransitRepository
    {
        public Task<WarehouseInventoryDto> GetInventoryFromWarehouse(string inventoryId, string warehouseId);
        public Task<bool> UpdateWarehouseStocks(WarehouseInventoryDto inventory);
        public Task<bool> AddWarehouseStocks(WarehouseInventoryDto inventory);
    }
}
