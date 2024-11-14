using accounting.DTOs;

namespace accounting.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<bool> CheckSiteAsync(Guid siteId);
        Task<bool> OnboardProductAsync(ProductDTO product, AccountDTO sourceAccount, AccountDTO sinkAccount, AccountDTO transitAccount);
    }
}
