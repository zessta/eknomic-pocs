using accounting.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace accounting.DTOs
{
    public class TransferRequestDTO
    {
        public Guid FromAccountId { get; set; }
        public Guid ToAccountId { get; set; }
        public int TransactionQuantity { get; set; }
    }
}
