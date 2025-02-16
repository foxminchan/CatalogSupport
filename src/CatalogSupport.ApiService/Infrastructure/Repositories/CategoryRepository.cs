using CatalogSupport.SharedKernel.SeedWork;

namespace CatalogSupport.ApiService.Infrastructure.Repositories;

public sealed class CategoryRepository(CatalogDbContext context) : ICategoryRepository
{
    private readonly CatalogDbContext _context =
        context ?? throw new ArgumentNullException(nameof(context));
    public IUnitOfWork UnitOfWork => _context;

    public async Task<Category> AddAsync(Category category)
    {
        var result = await _context.Categories.AddAsync(category);
        return result.Entity;
    }

    public async Task<IReadOnlyList<Category>> ListAsync() =>
        await _context.Categories.ToListAsync();
}
