﻿using InventoryManagement.Domain.DTO;
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Infrastructure.Repositories.Interfaces
{
    public interface IWarehouseRepository
    {
        Task<IEnumerable<Warehouse>> GetAllWarehousesAsync();
        Task<Warehouse> GetWarehouseByIdAsync(int id);
        Task AddWarehouseAsync(WarehouseDto warehouse);
        Task DeleteWarehouseAsync(int id);
    }
}
