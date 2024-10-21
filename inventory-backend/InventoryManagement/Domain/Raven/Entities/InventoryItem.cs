namespace InventoryManagement.Domain.Raven.Entities
{
    public class InventoryItem
    {
        public string Id { get; set; }
        public string Brand { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Features { get; set; }
        public string Details { get; set; }
        public string ItemClassificationId { get; set; }
        public string PackagingId { get; set; }
        public string OriginDetailsId { get; set; }
        public string DimensionsId { get; set; }
        public string ExpiryDetailsId { get; set; }
    }
}
