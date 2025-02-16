using CatalogSupport.SharedKernel.SeedWork;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CatalogSupport.ApiService.Features.Products.Create;

internal sealed class CreateProductEndpoint
    : IEndpoint<Created<Guid>, CreateProductCommand, ISender>
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/products",
                async ([AsParameters] CreateProductCommand command, ISender sender) =>
                    await HandleAsync(command, sender)
            )
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithOpenApi()
            .WithTags(nameof(Product))
            .WithFormOptions(bufferBody: true)
            .DisableAntiforgery();
    }

    public async Task<Created<Guid>> HandleAsync(
        CreateProductCommand command,
        ISender sender,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created(
            new UrlBuilder().WithResource(nameof(Products)).WithId(result).Build(),
            result
        );
    }
}
