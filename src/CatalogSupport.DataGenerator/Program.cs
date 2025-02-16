var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddSingleton<IAiService, AiService>();
builder.AddKeyedOllamaApiClient("ollama-deepseek-r1").AddKeyedChatClient();
builder.AddKeyedOllamaApiClient("ollama-llava").AddKeyedChatClient();
builder.AddOllamaApiClient("ollama-deepseek-r1").AddEmbeddingGenerator();

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
