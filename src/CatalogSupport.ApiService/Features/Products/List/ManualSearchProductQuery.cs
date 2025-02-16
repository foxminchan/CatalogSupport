using CatalogSupport.SharedKernel.SeedWork.Model;

namespace CatalogSupport.ApiService.Features.Products.List;

internal sealed record ManualSearchProductQuery(int PageIndex, int PageSize, string Query)
    : IQuery<PagedResult<ProductDto>>;

internal sealed class SearchProductHandler([AsParameters] ProductServices productServices)
    : IQueryHandler<ManualSearchProductQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(
        ManualSearchProductQuery request,
        CancellationToken cancellationToken
    )
    {
        var products = await productServices.ProductRepository.ListAsync(
            new(request.Query, (request.PageIndex - 1) * request.PageSize, request.PageSize)
        );

        var totalRecords = await productServices.ProductRepository.CountAsync(new(request.Query));

        var totalPages = (int)Math.Ceiling(totalRecords / request.PageSize);

        var result = productServices.Mapper.MapToDtos(products);

        return new(result, request.PageIndex, request.PageSize, totalRecords, totalPages);
    }
}
