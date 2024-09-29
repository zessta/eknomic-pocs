using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagement.Models.Entities
{
    public class InventoryStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int InventoryItemId { get; set; }
        public virtual InventoryItem InventoryItem { get; set; }

        [Required]
        public int LocationId { get; set; }
        public virtual Location Location { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public DateTime StatusChangeDate { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string MovedBy { get; set; }
    }
}
