namespace InventoryManagement.Domain.Events
{
    public class TransferEvent
    {
        public int SourceWarehouseId { get; set; }
        public int DestinationWarehouseId { get; set; }
        public int InventoryId { get; set; }
        public int Quantity { get; set; }
    }
}
