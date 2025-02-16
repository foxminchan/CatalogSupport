namespace CatalogSupport.ApiService.Features.Products;

public static class DomainToDtoMapper
{
    public static ProductDto ToProductDto(this Product product, string? imageUrl)
    {
        return new(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            imageUrl,
            product.CategoryId
        );
    }

    public static IReadOnlyList<ProductDto> ToProductDtos(
        this IReadOnlyList<Product> products,
        Func<Product, string?> imageUrlProvider
    )
    {
        return products
            .Select(product => ToProductDto(product, imageUrlProvider(product)))
            .ToList();
    }
}
