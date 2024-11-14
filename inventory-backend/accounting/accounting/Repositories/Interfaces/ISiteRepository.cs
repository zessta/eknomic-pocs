using accounting.DTOs;

namespace accounting.Repositories.Interfaces
{
    public interface ISiteRepository
    {
        Task CreateSite(SiteDTO site);
    }
}
