using System.Diagnostics;
using CatalogSupport.SharedKernel.Mediator;
using CatalogSupport.SharedKernel.SeedWork;
using MediatR;
using Polly;

namespace CatalogSupport.ApiService.Infrastructure;

public sealed class CatalogDbContext : DbContext, IUnitOfWork
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    private readonly IPublisher _publisher;

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options, IPublisher publisher)
        : base(options)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        Debug.WriteLine($"CatalogDbContext::ctor -> {GetHashCode()}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await _publisher.DispatchDomainEventsAsync(this);
        await SaveChangesAsync(cancellationToken);
        return true;
    }

    public static async Task EnsureDbCreatedAsync(
        IServiceProvider services,
        string? initialImportDataDir
    )
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new() { Delay = TimeSpan.FromSeconds(3) })
            .Build();
        var createdDb = await pipeline.ExecuteAsync(async ct =>
        {
            await dbContext.Database.MigrateAsync(cancellationToken: ct);
            return await dbContext.Database.EnsureCreatedAsync(ct);
        });

        if (createdDb && !string.IsNullOrWhiteSpace(initialImportDataDir))
        {
            await ImportDataAsync(dbContext, initialImportDataDir);
        }
    }

    private static async Task ImportDataAsync(
        CatalogDbContext dbContext,
        string initialImportDataDir
    )
    {
        // Checks if the specified directory exists and contains JSON files.
        if (
            !Directory.Exists(initialImportDataDir)
            || Directory
                .GetFiles(initialImportDataDir, "*.json", SearchOption.TopDirectoryOnly)
                .Length == 0
        )
        {
            return;
        }

        try
        {
            var categories =
                JsonSerializer.Deserialize<Category[]>(
                    await File.ReadAllTextAsync(
                        Path.Combine(initialImportDataDir, "categories.json")
                    )
                ) ?? [];

            var products =
                JsonSerializer.Deserialize<Product[]>(
                    await File.ReadAllTextAsync(Path.Combine(initialImportDataDir, "products.json"))
                ) ?? [];

            await dbContext.Categories.AddRangeAsync(categories);
            await dbContext.Products.AddRangeAsync(products);
            await dbContext.SaveEntitiesAsync();
        }
        catch (Exception)
        {
            await dbContext.Database.EnsureDeletedAsync();
            throw;
        }
    }
}
