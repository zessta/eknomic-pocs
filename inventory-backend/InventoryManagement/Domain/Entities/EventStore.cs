using InventoryManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace InventoryManagement.Domain.Entities
{
    public class EventStore
    {
        [Key]
        public int EventId {  get; set; }
        public InventoryEvents EventType { get; set; }
        public string? EventData { get; set; }
        public DateTime OccuredTimeUtc { get; set; } = DateTime.UtcNow;

        public int WarehouseId { get; set; }
        [JsonIgnore]
        [ForeignKey("WarehouseId")]
        public Warehouse Warehouse { get; set; }
    }
}
