using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace accounting.Entities
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("product_id")]
        public Guid ProductId { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("brand")]
        public string Brand { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("features")]
        public string Features { get; set; }

        [Column("details")]
        public string Details { get; set; }
    }
}
