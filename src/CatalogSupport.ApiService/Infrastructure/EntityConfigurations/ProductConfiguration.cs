using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CatalogSupport.ApiService.Infrastructure.EntityConfigurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);

        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);

        builder.Property(x => x.Price).HasPrecision(18, 2).IsRequired();

        builder.Property(x => x.Image).HasMaxLength(100);

        builder.Property(e => e.CreatedAt).HasDefaultValue(DateTime.UtcNow);

        builder.Property(e => e.LastModifiedAt).HasDefaultValue(DateTime.UtcNow);

        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.HasIndex(e => e.IsDeleted);

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder
            .HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(e => e.Category).AutoInclude();
    }
}
