using accounting.Entities;
using accounting.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace accounting.DTOs
{
    public class AccountDTO
    {
        public Guid AccountId { get; set; }
        public Guid ProductId { get; set; }
        public Guid SiteId { get; set; }
        public AccountType AccountType { get; set; }
        public int AvailableQuantity { get; set; }
    }
}
