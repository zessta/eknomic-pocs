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

        public async Task<EventStore> TransitInventory(TransferEvent transferEvent)
        {
            var sourceInventory = await _transitRepository.GetInventoryFromWarehouse(transferEvent.InventoryId, transferEvent.SourceWarehouseId);
            var destinationInventory = await _transitRepository.GetInventoryFromWarehouse(transferEvent.InventoryId, transferEvent.DestinationWarehouseId);
            if (sourceInventory != null && sourceInventory.Quantity > transferEvent.Quantity)
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

                return await _transitRepository.RaiseEvent(InventoryEvents.Transfer, transferEvent);

            }
            return null;
        }


    }
}
