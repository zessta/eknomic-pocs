using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Domain.DTO
{
    public class InventoryDto
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Features { get; set; }
        public string Details { get; set; }
        public string Segment { get; set; }
        public string Category { get; set; }
        public string ClassificationType { get; set; }
        public decimal Height { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public decimal Weight { get; set; }
        public string PackagingType { get; set; }
        public int QuantityPerPackage { get; set; }
        public string CountryOfOrigin { get; set; }
        public string ManufacturerName { get; set; }
        public string ManufacturerDetails { get; set; }
        public string SupplierName { get; set; }
        public string SupplierContact { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
