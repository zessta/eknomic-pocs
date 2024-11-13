using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace InventoryManagement.Domain.Entities
{
    public class Warehouse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WarehouseId { get; set; }

        
        [MaxLength(255)]
        public required string Location { get; set; }

        public Guid ManagerId {  get; set; }

        [JsonIgnore]
        [ForeignKey("ManagerId")]
        public User Users { get; set; }
    }
}
    