namespace CatalogSupport.SharedKernel.SeedWork;

public sealed class UrlBuilder
{
    private string? _resource;
    private string? _id;

    public UrlBuilder WithResource(string resource)
    {
        _resource = resource.ToLowerInvariant();
        return this;
    }

    public UrlBuilder WithId<T>(T id)
    {
        _id = id?.ToString();
        return this;
    }

    public string Build()
    {
        return $"/api/{_resource}/{_id}";
    }
}
