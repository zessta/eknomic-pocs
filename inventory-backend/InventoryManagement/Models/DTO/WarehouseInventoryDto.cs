namespace InventoryManagement.Models.DTO
{
    public class WarehouseInventoryDto
    {
        public int WarehouseInventoryId { get; set; }

        public InventoryDto InventoryItem { get; set; }
        public int Quantity { get; set; }
    }

}
