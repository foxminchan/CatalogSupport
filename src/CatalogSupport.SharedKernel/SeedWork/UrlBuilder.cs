namespace CatalogSupport.SharedKernel.SeedWork;

/// <summary>
/// A builder class for constructing URLs for API resources.
/// </summary>
public sealed class UrlBuilder
{
    private string? _resource;
    private string? _id;

    /// <summary>
    /// Sets the resource part of the URL.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <returns>The current instance of <see cref="UrlBuilder"/>.</returns>
    public UrlBuilder WithResource(string resource)
    {
        _resource = resource.ToLowerInvariant();
        return this;
    }

    /// <summary>
    /// Sets the ID part of the URL.
    /// </summary>
    /// <typeparam name="T">The type of the ID.</typeparam>
    /// <param name="id">The ID value.</param>
    /// <returns>The current instance of <see cref="UrlBuilder"/>.</returns>
    public UrlBuilder WithId<T>(T id)
    {
        _id = id?.ToString();
        return this;
    }

    /// <summary>
    /// Builds the URL string.
    /// </summary>
    /// <returns>The constructed URL string.</returns>
    public string Build()
    {
        return $"/api/{_resource}/{_id}";
    }
}
