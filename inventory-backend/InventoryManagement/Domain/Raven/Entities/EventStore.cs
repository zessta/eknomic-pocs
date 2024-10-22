using InventoryManagement.Domain.Enums;

namespace InventoryManagement.Domain.Raven.Entities
{
    public class EventStore
    {
        public string Id { get; set; }
        public InventoryEvents EventType { get; set; }
        public string? EventData { get; set; }
        public DateTime OccuredTimeUtc { get; set; } = DateTime.UtcNow;
        public string WarehouseId { get; set; }
    }
}
