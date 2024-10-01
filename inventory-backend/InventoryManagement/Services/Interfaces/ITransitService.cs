using InventoryManagement.Models.Entities;
using InventoryManagement.Models.Events;

namespace InventoryManagement.Services.Interfaces
{
    public interface ITransitService
    {
        public Task<EventStore> TransitInventory(TransferEvent transferEvent);
    }
}
