using CatalogSupport.SharedKernel.SeedWork;
using Microsoft.AspNetCore.Mvc;

namespace CatalogSupport.ApiService.Domain.AggregatesModel.ProductAggregate;

public sealed class ProductServices(
    [FromServices] IProductRepository productRepository,
    [FromServices] IMapper<Product, ProductDto> mapper
)
{
    public IProductRepository ProductRepository { get; } = productRepository;
    public IMapper<Product, ProductDto> Mapper { get; } = mapper;
}
