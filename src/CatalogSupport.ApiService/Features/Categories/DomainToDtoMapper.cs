namespace CatalogSupport.ApiService.Features.Categories;

public static class DomainToDtoMapper
{
    public static CategoryDto ToCategoryDto(this Category category)
    {
        return new(category.Id, category.Name);
    }

    public static IReadOnlyList<CategoryDto> ToCategoryDtos(this IReadOnlyList<Category> categories)
    {
        return categories.Select(ToCategoryDto).ToList();
    }
}
