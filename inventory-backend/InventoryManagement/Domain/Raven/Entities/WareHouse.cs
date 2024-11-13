namespace InventoryManagement.Domain.Raven.Entities
{
    public class Warehouse
    {
        public string Id { get; set; }
        public required string Location { get; set; }
        public Guid ManagerId { get; set; }
    }
}
