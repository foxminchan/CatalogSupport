using CatalogSupport.ApiService.Domain.AggregatesModel.ProductAggregate.Specifications;
using CatalogSupport.SharedKernel.SeedWork;
using CatalogSupport.SharedKernel.Specifications;

namespace CatalogSupport.ApiService.Infrastructure.Repositories;

public sealed class ProductRepository(CatalogDbContext context) : IProductRepository
{
    private readonly CatalogDbContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Product> AddAsync(Product product)
    {
        var result = await _context.Products.AddAsync(product);
        return result.Entity;
    }

    public async Task<Product?> FindByIdAsync(Guid id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<IReadOnlyList<Product>> ListAsync(ProductFilterSpec spec)
    {
        var query = SpecificationEvaluator.GetQuery(_context.Products.AsQueryable(), spec);
        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<double> CountAsync(ProductFilterSpec spec)
    {
        var query = SpecificationEvaluator.GetQuery(_context.Products.AsQueryable(), spec);
        return await query.AsNoTracking().CountAsync();
    }
}
