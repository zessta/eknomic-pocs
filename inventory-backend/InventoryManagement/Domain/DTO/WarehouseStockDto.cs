namespace InventoryManagement.Domain.DTO
{
    public class WarehouseStockDto
    {
        public string WarehouseInventoryId { get; set; }

        public InventoryDto InventoryItem { get; set; }
        public int Quantity { get; set; }
    }

}
