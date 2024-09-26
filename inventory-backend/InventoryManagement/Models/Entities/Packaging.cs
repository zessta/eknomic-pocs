using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace InventoryManagement.Models.Entities
{
    public class Packaging
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public int QuantityPerPackage { get; set; }

        // Relationships
        [JsonIgnore]
        public virtual List<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    }

}
