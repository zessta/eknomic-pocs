using accounting.DTOs;
using accounting.Repositories.Interfaces;
using accounting.Services.Interfaces;

namespace accounting.Services
{
    public class SiteService : ISiteService
    {
        private readonly ISiteRepository _siteRepository;
        public SiteService(ISiteRepository siteRepository)
        {
            _siteRepository = siteRepository;
        }
        public async Task OnBoardSite(SiteDTO site)
        {
            await _siteRepository.CreateSite(site);
        }
    }
}
