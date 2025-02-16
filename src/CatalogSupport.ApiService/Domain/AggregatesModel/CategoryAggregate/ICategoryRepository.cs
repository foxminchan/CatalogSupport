using CatalogSupport.SharedKernel.SeedWork;

namespace CatalogSupport.ApiService.Domain.AggregatesModel.CategoryAggregate;

public interface ICategoryRepository : IRepository
{
    Task<Category> AddAsync(Category category);
    Task<IReadOnlyList<Category>> ListAsync();
}
