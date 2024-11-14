using accounting.DTOs;

namespace accounting.Services.Interfaces
{
    public interface ISiteService
    {
        Task OnBoardSite(SiteDTO site);
    }
}
