namespace InventoryManagement.Domain.Events
{
    public class TransferEvent
    {
        public string SourceWarehouseId { get; set; }
        public string DestinationWarehouseId { get; set; }
        public string InventoryId { get; set; }
        public int Quantity { get; set; }
    }
}
