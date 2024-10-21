using InventoryManagement.Domain.DTO;
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

            await session.StoreAsync(newWarehouse); // Store the new document
            await session.SaveChangesAsync(); // Commit the changes
        }

        public async Task DeleteWarehouseAsync(string id)
        {
            using var session = _context.AsyncSession;
            var warehouse = await session.LoadAsync<Warehouse>(id); // Load by ID
            if (warehouse != null)
            {
                session.Delete(warehouse); // Mark the document for deletion
                await session.SaveChangesAsync(); // Commit the changes
            }
        }

        public async Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync()
        {
            using var session = _context.AsyncSession;
            return await session.Query<Warehouse>()
            .Select(x => new WarehouseDto
            {
                WarehouseId = x.Id, // RavenDB automatically generates an Id for each document
                Location = x.Location,
                ManagerId = x.ManagerId
            })
            .ToListAsync();
        }

        public async Task<WarehouseDto> GetWarehouseByIdAsync(string id)
        {
            using var session = _context.AsyncSession;
            var warehouse = await session.LoadAsync<Warehouse>(id); // Load by ID

            return new WarehouseDto
            {
                WarehouseId = warehouse.Id,
                Location = warehouse.Location,
                ManagerId = warehouse.ManagerId
            };
        }
    }
}
