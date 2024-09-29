namespace InventoryManagement.Models
{
    public class WarehouseInventoryDto
    {
        public int WarehouseInventoryId { get; set; }

        public InventoryItemDto InventoryItem { get; set; } // Reuse InventoryItemDto
        public int Quantity { get; set; }
    }

}
