using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Events;

namespace InventoryManagement.Infrastructure.Services.Interfaces
{
    public interface ITransitService
    {
        public Task<EventStore> TransitInventory(TransferEvent transferEvent);
    }
}
