using CatalogSupport.SharedKernel.SeedWork.Model;

namespace CatalogSupport.ApiService.Features.Products.List;

internal sealed record SemanticSearchProductQuery(int PageIndex, int PageSize, string Query)
    : IQuery<PagedResult<ProductDto>>;

internal sealed class SemanticSearchProductHandler(
    ProductSemanticSearch productSemanticSearch,
    [AsParameters] ProductServices productServices
) : IQueryHandler<SemanticSearchProductQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(
        SemanticSearchProductQuery request,
        CancellationToken cancellationToken
    )
    {
        var ids = await productSemanticSearch.FindProductsAsync(request.Query, cancellationToken);

        var products = await productServices.ProductRepository.ListAsync(
            new(ids, (request.PageIndex - 1) * request.PageSize, request.PageSize)
        );

        var totalRecords = await productServices.ProductRepository.CountAsync(new(ids));

        var totalPages = (int)Math.Ceiling(totalRecords / request.PageSize);

        var result = productServices.Mapper.MapToDtos(products);

        return new(result, request.PageIndex, request.PageSize, totalRecords, totalPages);
    }
}
