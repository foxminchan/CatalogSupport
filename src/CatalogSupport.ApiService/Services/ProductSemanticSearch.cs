using Qdrant.Client.Grpc;

namespace CatalogSupport.ApiService.Services;

public sealed class ProductSemanticSearch([AsParameters] AiServices aiServices)
{
    private static readonly string _collection = nameof(Product).ToLowerInvariant();

    public async Task<Guid[]> FindProductsAsync(
        string searchText,
        CancellationToken cancellationToken = default
    )
    {
        var results = new List<Guid>();

        var embeddings = await aiServices.EmbeddingService.GetEmbeddingAsync(
            searchText,
            cancellationToken
        );

        var scoredPoints = await aiServices.QdrantClient.SearchAsync(
            _collection,
            embeddings.ToArray(),
            cancellationToken: cancellationToken
        );

        results.AddRange(scoredPoints.Select(scoredPoint => Guid.Parse(scoredPoint.Id.Uuid)));

        return [.. results];
    }

    public static async Task EnsureSeedDataImportedAsync(
        IServiceProvider services,
        string? initialImportDataDir
    )
    {
        if (!string.IsNullOrEmpty(initialImportDataDir))
        {
            using var scope = services.CreateScope();
            await ImportProductSeedDataAsync(scope, initialImportDataDir);
        }
    }

    private static async Task ImportProductSeedDataAsync(
        IServiceScope scope,
        string initialImportDataDir
    )
    {
        var aiServices = scope.ServiceProvider.GetRequiredService<AiServices>();

        var products =
            JsonSerializer.Deserialize<Product[]>(
                await File.ReadAllTextAsync(Path.Combine(initialImportDataDir, "products.json"))
            ) ?? [];

        if (!await aiServices.QdrantClient.CollectionExistsAsync(_collection))
        {
            await aiServices.QdrantClient.CreateCollectionAsync(
                _collection,
                new VectorParams { Size = 768, Distance = Distance.Cosine }
            );
        }

        var points = products.Select(product => new PointStruct
        {
            Id = product.Id,
            Vectors = product.Embedding.ToArray(),
        });

        await aiServices.QdrantClient.UpsertAsync(_collection, [.. points]);
    }
}
