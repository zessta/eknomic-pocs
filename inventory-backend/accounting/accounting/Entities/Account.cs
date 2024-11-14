using accounting.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace accounting.Entities
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("account_id")]
        public Guid AccountId { get; set; }

        [ForeignKey("product_id")]
        public Guid ProductId { get; set; }
        public Product Product { get; set; }
        
        [ForeignKey("site_id")]
        public Guid SiteId { get; set; }
        public Site Site { get; set; }

        [Column("account_type")]
        public AccountType AccountType { get; set; }

        [Column("available_quantity")]
        public int AvailableQuantity { get; set; }
    }
}
