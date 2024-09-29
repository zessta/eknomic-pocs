using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models.Entities
{
    public class WarehouseInventory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WarehouseInventoryId { get; set; }

        [Required]
        public int WarehouseId { get; set; }
        public virtual Warehouse Warehouse { get; set; }

        [Required]
        public int InventoryItemId { get; set; }
        public virtual InventoryItem InventoryItem { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}
