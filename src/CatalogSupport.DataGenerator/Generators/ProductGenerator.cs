using CatalogSupport.DataGenerator.Models;
using CatalogSupport.DataGenerator.Shared;

namespace CatalogSupport.DataGenerator.Generators;

public sealed class ProductGenerator(IReadOnlyList<Category> categories, IServiceProvider services)
    : Generator<Product>(services)
{
    protected override string DirectoryName => "products";

    protected override Guid GetId(Product item) => item.Id;

    protected override async IAsyncEnumerable<Product> GenerateCoreAsync()
    {
        if (Directory.GetFiles(OutputDirPath).Length != 0)
        {
            yield break;
        }

        const int numProducts = 200;
        const int batchSize = 5;

        Logger.LogInformation(
            "Generating {numProducts} products in batches of {batchSize}...",
            numProducts,
            batchSize
        );

        var mappedBatches = MapParallel(
            Enumerable.Range(0, numProducts / batchSize),
            async _ =>
            {
                var chosenCategories = Enumerable
                    .Range(0, batchSize)
                    .Select(_ =>
                        categories[(int)Math.Floor(categories.Count * Random.Shared.NextDouble())]
                    )
                    .ToList();

                var prompt = $$$"""
                Write list of {{{batchSize}}} products for an online retailer of outdoor adventure goods and related clothing. 
                There is a focus on sustainability and environmental responsibility. They match the following category pairs:
                {{{string.Join(
                    Environment.NewLine,
                    chosenCategories.Select(
                        (c, index) => $"- product {(index + 1)}: category {c.Name}"
                    )
                )}}}
                
                Name are up to 100 characters long, but usually shorter. Sometimes they include numbers, specs, or product codes.
                Example model names: "Ultra Stretch DRY-EX Full-Zip Hoodie | UV Protection", "Wide Tapered Jeans", "Cotton Twill Work Jacket", "Over-sized T-Shirt | Bi-color".
                Do not repeat the category name in the product name.
                
                The description is up to 200 characters long and is the marketing text that will appear on the product page.
                Include the key features and selling points.
                
                Leave the image field empty for now.
                
                The result should be JSON form {{ "products": [{{ "id": "guid", "name": "string", "description": "string", "image": null, "price": 123.45 }}, ...] }}.";
                """;

                var response = await GetAndParseJsonChatCompletion<Response>(
                    prompt,
                    maxTokens: 200 * batchSize
                );
                var batchEntryIndex = 0;

                foreach (var p in response.Products!)
                {
                    var category = chosenCategories[batchEntryIndex++];
                    p.CategoryId = category.Id;
                }

                return response.Products;
            }
        );

        await foreach (var batch in mappedBatches)
        {
            foreach (var p in batch)
            {
                p.Id = Guid.CreateVersion7();
                yield return p;
            }
        }
    }

    internal sealed class Response
    {
        public List<Product>? Products { get; set; }
    }
}
