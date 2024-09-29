using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace InventoryManagement.Models.Entities
{
    public class OriginDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string CountryOfOrigin { get; set; }

        [Required]
        public string ManufacturerName { get; set; }

        public string ManufacturerDetails { get; set; }

        [Required]
        public string SupplierName { get; set; }

        public string SupplierContact { get; set; }

        // Relationships
        [JsonIgnore]
        public virtual List<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    }

}
