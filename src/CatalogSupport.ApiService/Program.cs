using CatalogSupport.ApiService.Extensions;
using CatalogSupport.ServiceDefaults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add application services.
builder.AddApplicationServices();

// Add services to the container.
builder.Services.AddProblemDetails();

// Add OpenAPI services.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer(
        (document, _, _) =>
        {
            document.Servers = [];
            document.Info.Title = "Catalog Support";
            document.Info.Description = "A POC for demonstrating the use of GenAI applications.";
            return Task.CompletedTask;
        }
    );
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.Title = "Catalog Support";
        options.DefaultFonts = false;
    });

    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.MapDefaultEndpoints();

app.MapEndpoints();

var initialImportDataDir = args.Length > 0 ? args[0] : null;

await CatalogDbContext.EnsureDbCreatedAsync(app.Services, initialImportDataDir);
await ProductSemanticSearch.EnsureSeedDataImportedAsync(app.Services, initialImportDataDir);
await ProductManualSemanticSearch.EnsureSeedDataImportedAsync(app.Services, initialImportDataDir);

app.Run();
