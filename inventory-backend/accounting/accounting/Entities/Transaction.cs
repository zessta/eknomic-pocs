using accounting.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace accounting.Entities
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("entry_id")]
        public Guid EntryId { get; set; }

        [ForeignKey("account_id")]
        public Guid AccountId { get; set; }
        public Account Account { get; set; }

        [Column("account_type")]
        public AccountType AccountType { get; set; }

        [Column("description")]
        public TransactionDescription Description { get; set; }

        [Column("transfer_type")]
        public TransferType TransferType { get; set; }

        [Column("transaction_quantity")]
        public int TransactionQuantity { get; set; }

        [Column("transaction_id")]
        public string TransactionId { get; set; }

        [ForeignKey("master_transaction_id")]
        public Guid MasterTransactionId { get; set; }
        public MasterTransaction MasterTransaction { get; set; }

        [Column("closing_balance")]
        public int ClosingBalance { get; set; }
    }
}
