using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Events;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Infrastructure.Repositories.Interfaces;
using InventoryManagement.Infrastructure.Services.Interfaces;

namespace InventoryManagement.Infrastructure.Services
{
    public class TransitService : ITransitService
    {
        private readonly ITransitRepository _transitRepository;
        public TransitService(ITransitRepository transitRepository)
        {
            _transitRepository = transitRepository;
        }

        /* first get inventory details of both src & dst
           validate inventory stock and update inventories
           raise an event to store actions
        */
        public async Task<(EventStore, EventStore)> TransitInventory(TransferEvent transferEvent)
        {
           var (sourceInventory, destinationInventory) = await GetInventories(transferEvent);
            if (sourceInventory != null && sourceInventory.Quantity > transferEvent.Quantity)
            {
                await UpdateInventories(sourceInventory, destinationInventory, transferEvent);
                return await RaiseTransferEvent(transferEvent);

            }
            return (null, null);
        }

        private async Task<(WarehouseInventory, WarehouseInventory)> GetInventories(TransferEvent transferEvent)
        {
            var sourceInventory = await _transitRepository.GetInventoryFromWarehouse(transferEvent.InventoryId, transferEvent.SourceWarehouseId);
            var destinationInventory = await _transitRepository.GetInventoryFromWarehouse(transferEvent.InventoryId, transferEvent.DestinationWarehouseId);
            return (sourceInventory, destinationInventory);
        }

        private async Task UpdateInventories(WarehouseInventory sourceInventory, WarehouseInventory destinationInventory, TransferEvent transferEvent)
        {
            sourceInventory.Quantity -= transferEvent.Quantity;
            await _transitRepository.UpdateWarehouseStocks(sourceInventory);

            if (destinationInventory == null)
            {
                destinationInventory = new WarehouseInventory()
                {
                    WarehouseId = transferEvent.DestinationWarehouseId,
                    InventoryItemId = transferEvent.InventoryId,
                    Quantity = transferEvent.Quantity
                };
                await _transitRepository.AddWarehouseStocks(destinationInventory);
            }
            else
            {
                destinationInventory.Quantity += transferEvent.Quantity;
                await _transitRepository.UpdateWarehouseStocks(destinationInventory);
            }
        }

        private async Task<(EventStore, EventStore)> RaiseTransferEvent(TransferEvent transferEvent)
        {
            var reductionQuantity = $"- {transferEvent.Quantity}";
            var additionQuantity = $"+ {transferEvent.Quantity}";
            var reductionEvent = await _transitRepository.RaiseEvent(InventoryEvents.Reduction, reductionQuantity, transferEvent.SourceWarehouseId);
            var additionEvent = await _transitRepository.RaiseEvent(InventoryEvents.Addition, additionQuantity, transferEvent.DestinationWarehouseId);
            return (additionEvent, reductionEvent);
        }
    }
}
