using CatalogSupport.ApiService.Domain.AggregatesModel.ProductAggregate.Specifications;
using CatalogSupport.SharedKernel.SeedWork;

namespace CatalogSupport.ApiService.Domain.AggregatesModel.ProductAggregate;

public interface IProductRepository : IRepository
{
    Task<Product> AddAsync(Product product);
    Task<Product?> FindByIdAsync(Guid id);
    Task<IReadOnlyList<Product>> ListAsync(ProductFilterSpec spec);
    Task<double> CountAsync(ProductFilterSpec spec);
}
