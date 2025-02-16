using CatalogSupport.ApiService.Infrastructure.Blob;
using CatalogSupport.SharedKernel.SeedWork.Model;
using Qdrant.Client;

namespace CatalogSupport.ApiService.Features.Products.List;

internal sealed record SemanticSearchProductQuery(int PageIndex, int PageSize, string Query)
    : IQuery<PagedResult<ProductDto>>;

internal sealed class SemanticSearchProductHandler(
    IProductRepository productRepository,
    QdrantClient qdrantClient,
    IAiService aiService,
    IBlobService blobService
) : IQueryHandler<SemanticSearchProductQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(
        SemanticSearchProductQuery request,
        CancellationToken cancellationToken
    )
    {
        var embeddings = await aiService.GetEmbeddingAsync(request.Query, cancellationToken);

        var scoredPoints = await qdrantClient.SearchAsync(
            nameof(Product).ToLowerInvariant(),
            embeddings.ToArray(),
            cancellationToken: cancellationToken
        );

        var ids = scoredPoints.Select(scoredPoint => Guid.Parse(scoredPoint.Id.Uuid)).ToArray();

        var products = await productRepository.ListAsync(
            new(ids, (request.PageIndex - 1) * request.PageSize, request.PageSize)
        );

        var totalRecords = await productRepository.CountAsync(new(ids));

        var totalPages = (int)Math.Ceiling(totalRecords / request.PageSize);

        var result = products.ToProductDtos(product =>
            product.Image is not null ? blobService.GetFileUrl(product.Image) : null
        );

        return new(result, request.PageIndex, request.PageSize, totalRecords, totalPages);
    }
}
