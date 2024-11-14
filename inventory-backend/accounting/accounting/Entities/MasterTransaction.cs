using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace accounting.Entities
{
    public class MasterTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("master_transaction_id")]
        public Guid MasterTransactionId { get; set; }

        [ForeignKey("account_id")]
        public Guid FromAccountId { get; set; }
        public Account FromAccount { get; set; }

        [ForeignKey("account_id")]
        public Guid ToAccountId { get; set; }
        public Account ToAccount { get; set; }

        [Column("transaction_quantity")]
        public string TransactionQuantity { get; set; }
    }
}
