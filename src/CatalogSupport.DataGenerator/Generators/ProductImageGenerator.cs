using CatalogSupport.DataGenerator.Models;
using CatalogSupport.DataGenerator.Shared;

namespace CatalogSupport.DataGenerator.Generators;

public sealed class ProductImageGenerator(
    IReadOnlyList<Product> products,
    IServiceProvider services
) : Generator<IFormFile>(services, true)
{
    protected override string DirectoryName => "images";

    protected override Guid GetId(IFormFile item) => Guid.Empty;

    protected override IAsyncEnumerable<IFormFile> GenerateCoreAsync()
    {
        var productsToGenerated = products.Where(product =>
            string.IsNullOrWhiteSpace(product.Name)
        );
        return MapParallel(productsToGenerated, GenerateManualAsync);
    }

    private async Task<IFormFile> GenerateManualAsync(Product product)
    {
        var prompt = $"""
            Generate an image for the product with the following details:
            Name: {product.Name}
            Description: {product.Description}
            The image should be less than 1MB in size and the result should be in base64 text format.
            """;

        var response = await GetAndParseJsonChatCompletion<Response>(prompt, maxTokens: 70);

        if (response.Base64Images is null)
        {
            return null!;
        }

        var file = ConvertBase64ToImage(response.Base64Images, product.Name);

        return file;
    }

    private static FormFile ConvertBase64ToImage(string base64String, string name)
    {
        var bytes = Convert.FromBase64String(base64String);
        using var stream = new MemoryStream(bytes);
        return new(stream, 0, bytes.Length, name, $"{name}.png");
    }

    internal sealed class Response
    {
        public string? Base64Images { get; set; }
    }
}
