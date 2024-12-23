﻿using InventoryManagement.Domain.DTO;
using InventoryManagement.Domain.Raven.Entities;
using InventoryManagement.Infrastructure.Data.Raven;
using InventoryManagement.Infrastructure.Repositories.Interfaces;
using Raven.Client.Documents;

namespace InventoryManagement.Infrastructure.Repositories.Raven
{
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly RavenDbContext _context;
        public WarehouseRepository(RavenDbContext context)
        {
            _context = context;
        }

        public async Task AddWarehouseAsync(WarehouseDto warehouse)
        {
            using var session = _context.AsyncSession;
            var newWarehouse = new Warehouse
            {
                Location = warehouse.Location,
                ManagerId = warehouse.ManagerId
            };

            await session.StoreAsync(newWarehouse);
            await session.SaveChangesAsync();
        }

        public async Task DeleteWarehouseAsync(string id)
        {
            using var session = _context.AsyncSession;
            var warehouse = await session.LoadAsync<Warehouse>(id);
            if (warehouse != null)
            {
                session.Delete(warehouse);
                await session.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync()
        {
            using var session = _context.AsyncSession;
            return await session.Query<Warehouse>()
            .Select(x => new WarehouseDto
            {
                WarehouseId = x.Id,
                Location = x.Location,
                ManagerId = x.ManagerId
            })
            .ToListAsync();
        }

        public async Task<WarehouseDto> GetWarehouseByIdAsync(string id)
        {
            using var session = _context.AsyncSession;
            var warehouse = await session.LoadAsync<Warehouse>(id);

            return new WarehouseDto
            {
                WarehouseId = warehouse.Id,
                Location = warehouse.Location,
                ManagerId = warehouse.ManagerId
            };
        }
    }
}
