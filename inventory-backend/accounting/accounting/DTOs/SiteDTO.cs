using accounting.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace accounting.DTOs
{
    public class SiteDTO
    {
        public Guid SiteId { get; set; }
        public string SiteName { get; set; }
        public SiteType SiteType { get; set; }
        public string Location { get; set; }
    }
}
