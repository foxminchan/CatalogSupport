using System.Diagnostics;
using CatalogSupport.SharedKernel.Mediator;
using CatalogSupport.SharedKernel.SeedWork;
using MediatR;

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
}
