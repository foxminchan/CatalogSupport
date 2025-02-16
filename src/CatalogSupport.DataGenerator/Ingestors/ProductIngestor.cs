using System.Text.Json;
using CatalogSupport.DataGenerator.Models;

namespace CatalogSupport.DataGenerator.Ingestors;

public sealed class ProductIngestor(IServiceProvider services)
{
    private static string GeneratePath =>
        Path.Combine(Directory.GetCurrentDirectory(), "output", "products");
    private static readonly JsonSerializerOptions _inputOptions = new(JsonSerializerDefaults.Web);
    private static readonly JsonSerializerOptions _outputOptions = new() { WriteIndented = true };

    public async Task IngestAsync()
    {
        List<ProductWithEmbed> products = [];
        var embeddingGenerator = services.GetRequiredService<IAiService>();

        foreach (var filename in Directory.GetFiles(GeneratePath, "*.json"))
        {
            var generated = (
                await JsonSerializer.DeserializeAsync<GeneratedProduct>(
                    File.OpenRead(filename),
                    _inputOptions
                )
            )!;

            products.Add(
                new()
                {
                    Id = generated.Id,
                    CategoryId = generated.CategoryId,
                    Name = generated.Name,
                    Description = generated.Description,
                    Price = generated.Price,
                    Embedding = await embeddingGenerator.GetEmbeddingAsync(
                        $"{generated.Name} {generated.Description}"
                    ),
                }
            );
        }

        await File.WriteAllTextAsync(
            Path.Combine(GeneratePath, "products.json"),
            JsonSerializer.Serialize(products, _outputOptions)
        );
    }

    internal record GeneratedProduct(
        Guid Id,
        string Name,
        string Description,
        string? Image,
        decimal Price,
        Guid CategoryId
    );
}
