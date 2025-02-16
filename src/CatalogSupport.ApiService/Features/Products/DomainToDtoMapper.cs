using CatalogSupport.SharedKernel.SeedWork;

namespace CatalogSupport.ApiService.Features.Products;

public sealed class DomainToDtoMapper(IBlobService blobService) : IMapper<Product, ProductDto>
{
    public ProductDto MapToDto(Product product)
    {
        var imageUrl = product.Image is not null ? blobService.GetFileUrl(product.Image) : null;

        return new(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            imageUrl,
            product.CategoryId
        );
    }

    public IReadOnlyList<ProductDto> MapToDtos(IReadOnlyList<Product> products)
    {
        return products.Select(MapToDto).ToList();
    }
}

public static class Extensions
{
    public static IServiceCollection AddProductDomainToDtoMapper(this IServiceCollection services)
    {
        services.AddScoped<IMapper<Product, ProductDto>, DomainToDtoMapper>();
        return services;
    }
}
