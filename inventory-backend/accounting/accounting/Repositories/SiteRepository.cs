using accounting.Data;
using accounting.DTOs;
using accounting.Entities;
using accounting.Repositories.Interfaces;

namespace accounting.Repositories
{
    public class SiteRepository : ISiteRepository
    {
        private readonly AppDbContext _context;
        public SiteRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task CreateSite(SiteDTO site)
        {
            await _context.Sites.AddAsync(new Site
            {
                SiteId = Guid.NewGuid(),
                SiteName = site.SiteName,
                SiteType = site.SiteType,
                Location = site.Location
            });

            await _context.SaveChangesAsync();
        }
    }
}
