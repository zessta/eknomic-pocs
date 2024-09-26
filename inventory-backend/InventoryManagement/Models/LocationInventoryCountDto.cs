using InventoryManagement.Models.Entities;

namespace InventoryManagement.Models
{
    public class LocationInventoryCountDto
    {
        public Location Location { get; set; }
        public int InventoryCount { get; set; }
    }

}
