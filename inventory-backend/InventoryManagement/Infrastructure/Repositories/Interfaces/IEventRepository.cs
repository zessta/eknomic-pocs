using InventoryManagement.Domain.DTO;
using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Infrastructure.Repositories.Interfaces
{
    public interface IEventRepository
    {
        public Task<EventStoreDto> RaiseEvent<TEvent>(InventoryEvents eventType, TEvent eventData, string warehouseId);
    }
}
