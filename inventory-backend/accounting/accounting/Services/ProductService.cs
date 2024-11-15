using accounting.DTOs;
using accounting.Enums;
using accounting.Repositories.Interfaces;
using accounting.Services.Interfaces;

namespace accounting.Services
{
    public class ProductService(IProductRepository productRepository) : IProductService
    {
        private readonly IProductRepository _productRepository = productRepository;
        public async Task<bool> Addproduct(ProductDTO product, Guid siteId)
        {
            product.ProductId = Guid.NewGuid();
            if(siteId != Guid.Empty && await CheckSite(siteId))
            {
                var sourceAccount = CreateAccount(product.ProductId, siteId, AccountType.Source);
                var sinkAccount = CreateAccount(product.ProductId, siteId, AccountType.Sink);
                var transitAccount = CreateAccount(product.ProductId, siteId, AccountType.Transit);
                return await _productRepository.OnboardProductAsync(product, sourceAccount, sinkAccount, transitAccount);
            }
            return false;
        }

        public async Task<bool> CheckSite(Guid siteId)
        {
            return await _productRepository.CheckSiteAsync(siteId);
        }

        public AccountDTO CreateAccount(Guid productId, Guid siteId, AccountType accountType)
        {
            return new AccountDTO
            {
                AccountId = Guid.NewGuid(),
                ProductId = productId,
                SiteId = siteId,
                AccountType = accountType,
                AvailableQuantity = 0
            };
        }
    }
}
