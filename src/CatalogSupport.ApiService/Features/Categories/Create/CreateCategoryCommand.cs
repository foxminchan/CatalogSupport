namespace CatalogSupport.ApiService.Features.Categories.Create;

internal sealed record CreateCategoryCommand(string Name) : ICommand<Guid>;

internal sealed class CreateCategoryHandler(ICategoryRepository repository)
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        var category = new Category(request.Name);

        var result = await repository.AddAsync(category);

        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return result.Id;
    }
}
