using CatalogSupport.SharedKernel.SeedWork;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CatalogSupport.ApiService.Features.Categories.Create;

internal sealed class CreateCategoryEndpoint
    : IEndpoint<Created<Guid>, CreateCategoryCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/categories",
                async (CreateCategoryCommand request, ISender sender) =>
                    await HandleAsync(request, sender)
            )
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Category));
    }

    public async Task<Created<Guid>> HandleAsync(
        CreateCategoryCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created(
            new UrlBuilder().WithResource(nameof(Categories)).WithId(result).Build(),
            result
        );
    }
}
