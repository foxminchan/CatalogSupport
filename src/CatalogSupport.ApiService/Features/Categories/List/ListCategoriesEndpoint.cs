using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CatalogSupport.ApiService.Features.Categories.List;

internal sealed class ListCategoriesEndpoint : IEndpoint<Ok<IReadOnlyList<CategoryDto>>, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", async (ISender sender) => await HandleAsync(sender))
            .Produces<IReadOnlyList<CategoryDto>>()
            .WithOpenApi()
            .WithTags(nameof(Category));
    }

    public async Task<Ok<IReadOnlyList<CategoryDto>>> HandleAsync(
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var categories = await sender.Send(new ListCategoriesQuery(), cancellationToken);
        return TypedResults.Ok(categories);
    }
}
