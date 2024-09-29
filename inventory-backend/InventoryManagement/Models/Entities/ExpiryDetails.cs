using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace InventoryManagement.Models.Entities
{
    public class ExpiryDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime? ManufacturingDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        // Relationships
        [JsonIgnore]
        public virtual List<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    }

}
