using CatalogSupport.ApiService.Domain.Exceptions;
using CatalogSupport.SharedKernel.SeedWork;
using CatalogSupport.SharedKernel.SeedWork.Model;

namespace CatalogSupport.ApiService.Domain.AggregatesModel.CategoryAggregate;

public sealed class Category() : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string? Name { get; set; }
    public bool IsDeleted { get; set; }

    public Category(string? name)
        : this()
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new CatalogDomainException("Category name is required");
    }

    public void Delete()
    {
        IsDeleted = true;
    }
}
