using CatalogSupport.SharedKernel.SeedWork.Model;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CatalogSupport.ApiService.Features.Products.List;

internal sealed record SearchParams(
    string Search = "",
    int PageIndex = 1,
    int PageSize = 20,
    bool IsSemantic = false
);

internal sealed class ListProductsEndpoint
    : IEndpoint<Ok<PagedResult<ProductDto>>, SearchParams, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/products/search",
                async ([AsParameters] SearchParams query, ISender sender) =>
                    await HandleAsync(query, sender)
            )
            .Produces<PagedResult<ProductDto>>()
            .WithOpenApi()
            .WithTags(nameof(Product));
    }

    public async Task<Ok<PagedResult<ProductDto>>> HandleAsync(
        SearchParams query,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        PagedResult<ProductDto> products;

        if (query.IsSemantic)
        {
            products = await sender.Send(
                new SemanticSearchProductQuery(query.PageIndex, query.PageSize, query.Search),
                cancellationToken
            );
        }
        else
        {
            products = await sender.Send(
                new ManualSearchProductQuery(query.PageIndex, query.PageSize, query.Search),
                cancellationToken
            );
        }

        return TypedResults.Ok(products);
    }
}
