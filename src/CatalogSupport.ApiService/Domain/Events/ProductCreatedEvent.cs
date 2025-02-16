using CatalogSupport.SharedKernel.SeedWork.Event;

namespace CatalogSupport.ApiService.Domain.Events;

public sealed class ProductCreatedEvent(Guid id, string name, string description) : DomainEvent
{
    public Guid Id { get; init; } = id;
    public string Name { get; init; } = name;
    public string Description { get; init; } = description;
}
