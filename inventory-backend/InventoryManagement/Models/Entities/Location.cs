using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace InventoryManagement.Models.Entities
{
    public class Location
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string LocationName { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        // Relationships
        [JsonIgnore]
        public virtual List<InventoryStatus> InventoryStatuses { get; set; } = new List<InventoryStatus>();
    }

}
