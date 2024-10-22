using InventoryManagement.Domain.DTO;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Raven.Entities;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Data.Raven;
using InventoryManagement.Infrastructure.Repositories.Interfaces;

namespace InventoryManagement.Infrastructure.Repositories.Raven
{
    public class EventRepository : IEventRepository
    {
        private readonly RavenDbContext _context;
        public EventRepository(RavenDbContext context)
        {
            _context = context;
        }
        public async Task<EventStoreDto> RaiseEvent<TEvent>(InventoryEvents eventType, TEvent eventData, string warehouseId)
        {
            using var session =  _context.AsyncSession;
            var eventDetails = new EventStore
            {
                EventType = eventType,
                EventData = eventData?.ToString(),
                WarehouseId = warehouseId
            };

            await session.StoreAsync(eventDetails);
            await session.SaveChangesAsync();

            return new EventStoreDto
            {
                EventId = eventDetails.EventId,
                EventType = eventType,
                EventData = eventData?.ToString(),
                WarehouseId = warehouseId
            };
        }
    }
}
