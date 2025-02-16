using CatalogSupport.DataGenerator.Models;
using CatalogSupport.DataGenerator.Shared;

namespace CatalogSupport.DataGenerator.Generators;

public sealed class CategoryGenerator(IServiceProvider services) : Generator<Category>(services)
{
    protected override string DirectoryName => "categories";

    protected override Guid GetId(Category item) => item.Id;

    protected override async IAsyncEnumerable<Category> GenerateCoreAsync()
    {
        if (Directory.GetFiles(OutputDirPath).Length != 0)
        {
            yield break;
        }

        const int numCategories = 50;
        const int batchSize = 25;
        var categoryNames = new HashSet<string>();

        while (categoryNames.Count < numCategories)
        {
            Logger.LogInformation(
                "Generating {numCategories} categories in batches of {batchSize}...",
                numCategories,
                batchSize
            );

            var prompt = $$$"""
                Generate {{{batchSize}}} product category names for an online retailer
                of high-tech outdoor adventure goods and related clothing.

                Each category name is a single descriptive term, so it does not use the word 'and'.
                Category names should be interesting and novel, e.g., "Loungewear & Pajamas", "Sweatshirts & Hoodies" or "Socks & Underwear", not simply "Pajamas" or "Hoodies".

                The response should be a JSON object of the form {{ "categories": [{{ "name": "Loungewear & Pajamas" }}, ...] }}.";
                """;

            var response = await GetAndParseJsonChatCompletion<Response>(
                prompt,
                maxTokens: 70 * batchSize
            );

            if (response.Categories is null)
            {
                continue;
            }

            foreach (var c in response.Categories.Where(c => categoryNames.Add(c.Name)))
            {
                c.Id = Guid.CreateVersion7();
                yield return c;
            }
        }
    }

    internal sealed class Response
    {
        public List<Category>? Categories { get; set; }
    }
}
