using System.ComponentModel.DataAnnotations.Schema;

namespace accounting.DTOs
{
    public class ProductDTO
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public string Features { get; set; }
        public string Details { get; set; }
    }
}
