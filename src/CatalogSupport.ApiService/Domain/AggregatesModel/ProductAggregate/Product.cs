using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using CatalogSupport.ApiService.Domain.Events;
using CatalogSupport.ApiService.Domain.Exceptions;
using CatalogSupport.SharedKernel.SeedWork;
using CatalogSupport.SharedKernel.SeedWork.Model;

namespace CatalogSupport.ApiService.Domain.AggregatesModel.ProductAggregate;

public sealed class Product() : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Image { get; private set; }
    public decimal Price { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public bool IsDeleted { get; set; }

    [NotMapped, JsonConverter(typeof(EmbeddingJsonConverter))]
    public required ReadOnlyMemory<float> Embedding { get; set; }

    public Product(string name, string description, string? image, decimal price, Guid categoryId)
        : this()
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new CatalogDomainException("Product name is required");
        Description = !string.IsNullOrWhiteSpace(description)
            ? description
            : throw new CatalogDomainException("Product description is required");
        Image = image;
        Price =
            price <= 0
                ? throw new CatalogDomainException("Product price must be greater than zero")
                : price;
        CategoryId = categoryId;
        RegisterDomainEvent(new ProductCreatedEvent(Id, Name, Description));
    }

    public void Delete()
    {
        IsDeleted = true;
    }

    internal class EmbeddingJsonConverter : JsonConverter<ReadOnlyMemory<float>>
    {
        public override ReadOnlyMemory<float> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new InvalidOperationException(
                    $"JSON deserialization failed because the value type was {reader.TokenType} but should be {JsonTokenType.String}"
                );
            }

            var bytes = reader.GetBytesFromBase64();
            var floats = MemoryMarshal.Cast<byte, float>(bytes);
            return floats.ToArray();
        }

        public override void Write(
            Utf8JsonWriter writer,
            ReadOnlyMemory<float> value,
            JsonSerializerOptions options
        )
        {
            var bytes = MemoryMarshal.AsBytes(value.Span);
            writer.WriteBase64StringValue(bytes);
        }
    }
}
