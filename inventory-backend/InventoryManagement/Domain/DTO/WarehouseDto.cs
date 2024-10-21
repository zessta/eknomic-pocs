using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Domain.DTO
{
    public class WarehouseDto
    {
        public string WarehouseId { get; set; }
        public required string Location { get; set; }

        public Guid ManagerId { get; set; }
    }
}
