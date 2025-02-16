using CatalogSupport.ApiService.Domain.Events;
using MediatR;
using Qdrant.Client.Grpc;

namespace CatalogSupport.ApiService.Domain.EventHandlers;

public sealed class ProductCreatedEventHandler([AsParameters] AiServices aiServices)
    : INotificationHandler<ProductCreatedEvent>
{
    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        var collectionName = nameof(Product).ToLowerInvariant();

        if (!await aiServices.QdrantClient.CollectionExistsAsync(collectionName, cancellationToken))
        {
            await aiServices.QdrantClient.CreateCollectionAsync(
                collectionName,
                new VectorParams { Size = 768, Distance = Distance.Cosine },
                cancellationToken: cancellationToken
            );
        }

        var embedding = await aiServices.EmbeddingService.GetEmbeddingAsync(
            $"{notification.Name} {notification.Description}",
            cancellationToken
        );

        var pointStruct = new PointStruct { Id = notification.Id, Vectors = embedding.ToArray() };

        await aiServices.QdrantClient.UpsertAsync(
            collectionName,
            [pointStruct],
            cancellationToken: cancellationToken
        );
    }
}
