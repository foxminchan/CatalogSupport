using Microsoft.AspNetCore.Mvc;
using Qdrant.Client;

namespace CatalogSupport.ApiService.Infrastructure.AI;

public sealed class AiServices(
    [FromServices] IAiService aiService,
    [FromServices] QdrantClient qdrantClient
)
{
    public IAiService EmbeddingService { get; } = aiService;
    public QdrantClient QdrantClient { get; } = qdrantClient;
}
