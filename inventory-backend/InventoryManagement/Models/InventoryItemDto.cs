using InventoryManagement.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models
{
    public class InventoryItemDto
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Features { get; set; }
        public string Details { get; set; }
        public ItemClassification ItemClassification { get; set; }
        public Packaging Packaging { get; set; }
        public OriginDetails OriginDetails { get; set; }
        public Dimensions Dimensions { get; set; }
        public ExpiryDetails ExpiryDetails { get; set; }
    }
}
