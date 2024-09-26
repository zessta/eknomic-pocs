using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models.Entities
{
    public class Warehouse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WarehouseId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Location { get; set; }

        [MaxLength(255)]
        public string Manager { get; set; }

        // Navigation property - One warehouse can store many InventoryItems
        public ICollection<WarehouseInventory> WarehouseInventories { get; set; }
    }
}
    