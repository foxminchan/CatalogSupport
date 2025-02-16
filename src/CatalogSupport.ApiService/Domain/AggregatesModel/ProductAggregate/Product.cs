using CatalogSupport.ApiService.Domain.Events;
using CatalogSupport.ApiService.Domain.Exceptions;
using CatalogSupport.SharedKernel.SeedWork;
using CatalogSupport.SharedKernel.SeedWork.Model;

namespace CatalogSupport.ApiService.Domain.AggregatesModel.ProductAggregate;

public sealed class Product() : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Image { get; private set; }
    public decimal Price { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public bool IsDeleted { get; set; }

    public Product(string name, string description, string? image, decimal price, Guid categoryId)
        : this()
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new CatalogDomainException("Product name is required");
        Description = !string.IsNullOrWhiteSpace(description)
            ? description
            : throw new CatalogDomainException("Product description is required");
        Image = image;
        Price =
            price <= 0
                ? throw new CatalogDomainException("Product price must be greater than zero")
                : price;
        CategoryId = categoryId;
        RegisterDomainEvent(new ProductCreatedEvent(Id, Name, Description));
    }

    public void Delete()
    {
        IsDeleted = true;
    }
}
