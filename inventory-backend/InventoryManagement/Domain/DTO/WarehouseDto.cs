using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Domain.DTO
{
    public class WarehouseDto
    {
        public required string Location { get; set; }

        public Guid ManagerId { get; set; }
    }
}
