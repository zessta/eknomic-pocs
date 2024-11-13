using InventoryManagement.Domain.DTO;

namespace InventoryManagement.Infrastructure.Repositories.Interfaces
{
    public interface IWarehouseRepository
    {
        Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync();
        Task<WarehouseDto> GetWarehouseByIdAsync(string id);
        Task AddWarehouseAsync(WarehouseDto warehouse);
        Task DeleteWarehouseAsync(string id);
    }
}
