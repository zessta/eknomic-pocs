using accounting.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace accounting.DTOs
{
    public class TransferRequestDTO
    {
        public Guid ProductId { get; set; }
        public Guid FromSitetId { get; set; }
        public Guid ToSiteId { get; set; }
        public int TransactionQuantity { get; set; }
    }
}
