using CatalogSupport.ApiService.Infrastructure.Blob;
using CatalogSupport.SharedKernel.SeedWork.Model;

namespace CatalogSupport.ApiService.Features.Products.List;

internal sealed record ManualSearchProductQuery(int PageIndex, int PageSize, string Query)
    : IQuery<PagedResult<ProductDto>>;

internal sealed class SearchProductHandler(
    IProductRepository productRepository,
    IBlobService blobService
) : IQueryHandler<ManualSearchProductQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(
        ManualSearchProductQuery request,
        CancellationToken cancellationToken
    )
    {
        var products = await productRepository.ListAsync(
            new(request.Query, (request.PageIndex - 1) * request.PageSize, request.PageSize)
        );

        var totalRecords = await productRepository.CountAsync(new(request.Query));

        var totalPages = (int)Math.Ceiling(totalRecords / request.PageSize);

        var result = products.ToProductDtos(product =>
            product.Image is not null ? blobService.GetFileUrl(product.Image) : null
        );

        return new(result, request.PageIndex, request.PageSize, totalRecords, totalPages);
    }
}
