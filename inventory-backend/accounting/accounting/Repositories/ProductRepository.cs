using accounting.Data;
using accounting.DTOs;
using accounting.Entities;
using accounting.Repositories.Interfaces;
using AutoMapper;

namespace accounting.Repositories
{
    public class ProductRepository(AppDbContext context, IMapper mapper) : IProductRepository
    {
        private readonly AppDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        public async Task<bool> CheckSiteAsync(Guid siteId)
        {
            var site = await _context.Sites.FindAsync(siteId);
            return site != null;
        }

        public async Task<bool> OnboardProductAsync(ProductDTO productDTO, AccountDTO sourceAccountDTO, AccountDTO sinkAccountDTO, AccountDTO transitAccountDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var (product, accounts) = (
                    _mapper.Map<Product>(productDTO),
                    new[]
                    {
                        _mapper.Map<Account>(sourceAccountDTO),
                        _mapper.Map<Account>(sinkAccountDTO),
                        _mapper.Map<Account>(transitAccountDTO)
                    }
                    );

                await _context.Products.AddAsync(product);
                await _context.Accounts.AddRangeAsync(accounts);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }

        }
    }
}
