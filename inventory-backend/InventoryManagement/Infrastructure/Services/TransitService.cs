using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Events;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Infrastructure.Repositories.Interfaces;
using InventoryManagement.Infrastructure.Services.Interfaces;
using InventoryManagement.Domain.DTO;

namespace InventoryManagement.Infrastructure.Services
{
    public class TransitService : ITransitService
    {
        private readonly ITransitRepository _transitRepository;
        public TransitService(ITransitRepository transitRepository)
        {
            _transitRepository = transitRepository;
        }

        public async Task<(EventStoreDto, EventStoreDto)> TransitInventory(TransferEvent transferEvent)
        {
           var (sourceInventory, destinationInventory) = await GetInventories(transferEvent);
            if (sourceInventory != null && sourceInventory.Quantity > transferEvent.Quantity)
            {
                await UpdateInventories(sourceInventory, destinationInventory, transferEvent);
                return await RaiseTransferEvent(transferEvent);

            }
            return (null, null);
        }

        private async Task<(WarehouseInventoryDto, WarehouseInventoryDto)> GetInventories(TransferEvent transferEvent)
        {
            var sourceInventory = await _transitRepository.GetInventoryFromWarehouse(transferEvent.InventoryId, transferEvent.SourceWarehouseId);
            var destinationInventory = await _transitRepository.GetInventoryFromWarehouse(transferEvent.InventoryId, transferEvent.DestinationWarehouseId);
            return (sourceInventory, destinationInventory);
        }

        private async Task UpdateInventories(WarehouseInventoryDto sourceInventory, WarehouseInventoryDto destinationInventory, TransferEvent transferEvent)
        {
            sourceInventory.Quantity -= transferEvent.Quantity;
            await _transitRepository.UpdateWarehouseStocks(sourceInventory);

            if (destinationInventory == null)
            {
                destinationInventory = new WarehouseInventoryDto()
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

        private async Task<(EventStoreDto, EventStoreDto)> RaiseTransferEvent(TransferEvent transferEvent)
        {
            var reductionQuantity = $"- {transferEvent.Quantity}";
            var additionQuantity = $"+ {transferEvent.Quantity}";
            var reductionEvent = await _transitRepository.RaiseEvent(InventoryEvents.Reduction, reductionQuantity, transferEvent.SourceWarehouseId);
            var additionEvent = await _transitRepository.RaiseEvent(InventoryEvents.Addition, additionQuantity, transferEvent.DestinationWarehouseId);
            return (additionEvent, reductionEvent);
        }
    }
}
