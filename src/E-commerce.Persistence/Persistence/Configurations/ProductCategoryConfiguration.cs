using E_commerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_commerce.Persistence.Persistence.Configurations;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pc => pc.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pc => pc.ImageUrl)
            .HasMaxLength(500);

        builder.Property(pc => pc.ParentCategoryId);

        builder.Property(pc => pc.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasMany(pc => pc.Products)
            .WithOne()
            .HasForeignKey("CategoryId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(pc => pc.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasQueryFilter(pc => !pc.IsDeleted);

        builder.HasIndex(pc => pc.Name);
        builder.HasIndex(pc => pc.ParentCategoryId);
    }
}
