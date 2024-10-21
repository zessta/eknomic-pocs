namespace InventoryManagement.Domain.DTO
{
    public class ExpiryDetailsDto
    {
        public string Id { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

}
