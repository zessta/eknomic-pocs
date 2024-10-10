using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace InventoryManagement.Domain.Entities
{
    public class InventoryItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Brand { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        // Additional fields
        public string Features { get; set; }
        public string Details { get; set; }

        // Relationships
        [Required]
        public int ItemClassificationId { get; set; }
        public virtual ItemClassification ItemClassification { get; set; }

        [Required]
        public int PackagingId { get; set; }
        public virtual Packaging Packaging { get; set; }

        [Required]
        public int OriginDetailsId { get; set; }
        public virtual OriginDetails OriginDetails { get; set; }

        [Required]
        public int DimensionsId { get; set; }
        public virtual Dimensions Dimensions { get; set; }

        [Required]
        public int ExpiryDetailsId { get; set; }
        public virtual ExpiryDetails ExpiryDetails { get; set; }
        [JsonIgnore]

        // Navigation property for related warehouses
        public ICollection<WarehouseInventory> WarehouseInventories { get; set; }
    }


}
