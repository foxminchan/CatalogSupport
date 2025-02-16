using CatalogSupport.SharedKernel.Specifications;

namespace CatalogSupport.ApiService.Domain.AggregatesModel.ProductAggregate.Specifications;

public sealed class ProductFilterSpec : Specification<Product>
{
    public ProductFilterSpec(string searchTerm, int skip, int take)
        : base(p => p.Name!.Contains(searchTerm) || p.Description!.Contains(searchTerm))
    {
        AddPagination(skip, take);
    }

    public ProductFilterSpec(Guid[] ids, int skip, int take)
        : base(p => ids.Contains(p.Id))
    {
        AddPagination(skip, take);
    }

    public ProductFilterSpec(string searchTerm)
        : base(p => p.Name!.Contains(searchTerm) || p.Description!.Contains(searchTerm)) { }

    public ProductFilterSpec(Guid[] ids)
        : base(p => ids.Contains(p.Id)) { }
}
