namespace CatalogSupport.ApiService.Extensions;

internal static class Extensions
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddEndpoints(typeof(ICatalogSupportApiMarker));

        services.AddSingleton(
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            }
        );

        // Add exception handlers
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Add database configuration
        services.AddDbContext<CatalogDbContext>(options =>
        {
            options
                .UseNpgsql(builder.Configuration.GetConnectionString("catalog-db"))
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
                );
        });
        builder.EnrichNpgsqlDbContext<CatalogDbContext>();

        services.AddMigration<CatalogDbContext>();

        builder.AddQdrantClient("vector-db");

        // Configure MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<ICatalogSupportApiMarker>();
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(ActivityBehavior<,>));
        });

        // Configure FluentValidation
        services.AddValidatorsFromAssemblyContaining<ICatalogSupportApiMarker>(
            includeInternalTypes: true
        );

        services.AddSingleton<IActivityScope, ActivityScope>();
        services.AddSingleton<CommandHandlerMetrics>();
        services.AddSingleton<QueryHandlerMetrics>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // Add AI services
        services.AddSingleton<IAiService, AiService>();
        services.AddScoped<AiServices>();
        builder.AddOllamaApiClient("ollama-deepseek-r1").AddEmbeddingGenerator();

        // Add Blob services
        builder.AddAzureBlobClient("blobs");
        builder.Services.AddSingleton<IBlobService, BlobService>();

        // Add Product services
        services.AddProductDomainToDtoMapper();
        services.AddScoped<ProductServices>();
        services.AddScoped<ProductSemanticSearch>();
    }
}
