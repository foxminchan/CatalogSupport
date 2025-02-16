using CatalogSupport.ApiService.Infrastructure.AI;
using CatalogSupport.DataGenerator.Generators;
using CatalogSupport.DataGenerator.Ingestors;
using CatalogSupport.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddSingleton<IAiService, AiService>();
builder.AddKeyedOllamaApiClient("ollama-deepseek-r1").AddKeyedChatClient();
builder.AddKeyedOllamaApiClient("ollama-llava").AddKeyedChatClient();
builder.Services.AddEmbeddingGenerator(sp => new OllamaEmbeddingGenerator(
    sp.GetRequiredService<IOllamaApiClient>().Uri,
    sp.GetRequiredService<IOllamaApiClient>().SelectedModel
));

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapPut(
    "/generator",
    async (IServiceProvider services) =>
    {
        var categories = await new CategoryGenerator(services).GenerateAsync();

        await new ProductGenerator(categories, services).GenerateAsync();

        return Results.NoContent();
    }
);

app.MapPut(
    "/ingestor",
    async (IServiceProvider services) =>
    {
        await new ProductIngestor(services).IngestAsync();

        return Results.NoContent();
    }
);

app.Run();
