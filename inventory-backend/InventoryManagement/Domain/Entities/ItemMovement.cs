using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Domain.Entities
{
    public class ItemMovement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemMovementId { get; set; }

        [Required]
        public int InventoryItemId { get; set; }
        public virtual InventoryItem InventoryItem { get; set; }

        [Required]
        public int FromWarehouseId { get; set; }
        public virtual Warehouse FromWarehouse { get; set; }

        [Required]
        public int ToWarehouseId { get; set; }
        public virtual Warehouse ToWarehouse { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime MovementDate { get; set; }
    }
}
