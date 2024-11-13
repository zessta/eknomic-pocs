namespace InventoryManagement.Domain.Raven.Entities
{
    public class WarehouseInventory
    {
        public string Id { get; set; }
        public string WarehouseId { get; set; }
        public string InventoryItemId { get; set; }
        public int Quantity { get; set; }
    }
}
