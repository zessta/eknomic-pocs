using accounting.DTOs;
using accounting.Enums;

namespace accounting.Services.Interfaces
{
    public interface IProductService
    {
        Task<bool> Addproduct(ProductDTO product, Guid siteId);
        Task<bool> CheckSite(Guid siteId);
        AccountDTO CreateAccount(Guid productId, Guid siteId, AccountType accountType);
    }
}
