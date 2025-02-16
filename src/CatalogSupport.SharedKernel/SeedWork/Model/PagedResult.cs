namespace CatalogSupport.SharedKernel.SeedWork.Model;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageIndex,
    int PageSize,
    double TotalItems,
    double TotalPages
)
{
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}
