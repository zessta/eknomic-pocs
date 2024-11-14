using accounting.Entities;
using accounting.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace accounting.DTOs
{
    public class TransactionDTO
    {
        public Guid EntryId { get; set; }
        public Guid AccountId { get; set; }
        public AccountType AccountType { get; set; }
        public TransactionDescription Description { get; set; }
        public TransferType TransferType { get; set; }
        public int TransactionQuantity { get; set; }
        public string TransactionId { get; set; }
        public Guid MasterTransactionId { get; set; }
        public int ClosingBalance { get; set; }
    }
}
