using InventoryManagement.Domain.DTO;
using InventoryManagement.Domain.Events;

namespace InventoryManagement.Infrastructure.Services.Interfaces
{
    public interface ITransitService
    {
        public Task<(EventStoreDto, EventStoreDto)> TransitInventory(TransferEvent transferEvent);
    }
}
