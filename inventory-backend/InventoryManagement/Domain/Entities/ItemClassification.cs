using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace InventoryManagement.Domain.Entities
{
    public class ItemClassification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Segment { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public string Type { get; set; }

        // Relationships
        [JsonIgnore]
        public virtual List<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    }
}
