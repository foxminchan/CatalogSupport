using Microsoft.AspNetCore.Mvc;

namespace CatalogSupport.ApiService.Features.Products.Create;

internal sealed record CreateProductCommand(
    [FromForm] string Name,
    [FromForm] string Description,
    IFormFile? Image,
    [FromForm] decimal Price,
    [FromForm] Guid CategoryId
) : ICommand<Guid>;

internal sealed class CreateProductHandler(IProductRepository repository, IBlobService blobService)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken
    )
    {
        string? image = null;
        if (request.Image is not null)
        {
            image = await blobService.UploadFileAsync(request.Image, cancellationToken);
        }

        var product = new Product(
            request.Name,
            request.Description,
            image,
            request.Price,
            request.CategoryId
        )
        {
            Embedding = default,
        };

        var result = await repository.AddAsync(product);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return result.Id;
    }
}
