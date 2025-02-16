namespace CatalogSupport.ApiService.Infrastructure.AI;

public interface IAiService
{
    ValueTask<ReadOnlyMemory<float>> GetEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default
    );
}
