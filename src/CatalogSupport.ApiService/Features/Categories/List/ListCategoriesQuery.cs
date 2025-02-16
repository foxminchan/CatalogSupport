namespace CatalogSupport.ApiService.Features.Categories.List;

internal sealed record ListCategoriesQuery : IQuery<IReadOnlyList<CategoryDto>>;

internal sealed class ListCategoriesHandler(ICategoryRepository categoryRepository)
    : IQueryHandler<ListCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public async Task<IReadOnlyList<CategoryDto>> Handle(
        ListCategoriesQuery request,
        CancellationToken cancellationToken
    )
    {
        var categories = await categoryRepository.ListAsync();

        return categories.ToCategoryDtos();
    }
}
