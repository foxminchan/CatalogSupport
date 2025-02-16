namespace CatalogSupport.ApiService.Features.Products;

public sealed record ProductDto(
    Guid Id,
    string? Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    Guid CategoryId
);
