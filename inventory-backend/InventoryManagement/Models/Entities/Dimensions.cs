using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace InventoryManagement.Models.Entities
{
    public class Dimensions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public decimal Height { get; set; }

        [Required]
        public decimal Width { get; set; }

        [Required]
        public decimal Length { get; set; }

        [Required]
        public decimal Weight { get; set; }

        // Relationships
        [JsonIgnore]
        public virtual List<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    }

}
