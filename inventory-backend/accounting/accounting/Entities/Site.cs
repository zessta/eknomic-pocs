using accounting.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace accounting.Entities
{
    public class Site
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("site_id")]
        public Guid SiteId { get; set; }

        [Column("site_name")]
        public string SiteName { get; set; }

        [Column("site_type")]
        public SiteType SiteType { get; set; }

        [Column("location")]
        public string Location { get; set; }
    }
}
