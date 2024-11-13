namespace InventoryManagement.Domain.DTO
{
    public class WarehouseInventoryDto
    {
        public string WarehouseInventoryId { get; set; }
        public string WarehouseId { get; set; }
        public string InventoryItemId { get; set; }
        public int Quantity { get; set; }
    }
}
