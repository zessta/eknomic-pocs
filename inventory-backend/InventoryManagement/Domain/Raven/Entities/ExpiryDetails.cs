namespace InventoryManagement.Domain.Raven.Entities
{
    public class ExpiryDetails
    {
        public string Id { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

}
