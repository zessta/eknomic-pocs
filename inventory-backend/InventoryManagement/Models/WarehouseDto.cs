namespace InventoryManagement.Models
{
    public class WarehouseDto
    {
        public int WarehouseId { get; set; }
        public string Location { get; set; }
        public string Manager { get; set; }

        // To represent associated inventory items in the warehouse
        public List<InventoryItemDto> InventoryItems { get; set; }
    }
}
