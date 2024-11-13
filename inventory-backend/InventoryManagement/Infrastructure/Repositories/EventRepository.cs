using InventoryManagement.Domain.DTO;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories.Interfaces;

namespace InventoryManagement.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly InventoryDbContext _context;
        public EventRepository(InventoryDbContext context)
        {
            _context = context;
        }
        public async Task<EventStoreDto> RaiseEvent<TEvent>(InventoryEvents eventType, TEvent eventData, string warehouseId)
        {
            var eventDetails = new EventStore()
            {
                EventType = eventType,
                EventData = eventData?.ToString(),
                WarehouseId = int.Parse(warehouseId)
            };

            await _context.EventStore.AddAsync(eventDetails);
            await _context.SaveChangesAsync();
            return new EventStoreDto
            {
                EventId = eventDetails.EventId.ToString(),
                EventType = eventType,
                EventData = eventData?.ToString(),
                WarehouseId = warehouseId
            };
        }
    }
}
