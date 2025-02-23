﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CatalogSupport.DataGenerator.Models;

public class Product
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string? Image { get; set; }
    public decimal Price { get; set; }
    public required Guid CategoryId { get; set; }
}

public sealed class ProductWithEmbed : Product
{
    [NotMapped, JsonConverter(typeof(EmbeddingJsonConverter))]
    public required ReadOnlyMemory<float> Embedding { get; set; }

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
