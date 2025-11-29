using E_commerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_commerce.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();

            email.HasIndex(e => e.Value).IsUnique();
        });

        builder.Property(c => c.PhoneNumber)
            .HasMaxLength(20);

        builder.OwnsOne(c => c.DefaultShippingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("ShippingStreet").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("ShippingCity").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("ShippingState").HasMaxLength(100);
            address.Property(a => a.ZipCode).HasColumnName("ShippingZipCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("ShippingCountry").HasMaxLength(100);
        });

        builder.OwnsOne(c => c.DefaultBillingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("BillingStreet").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("BillingCity").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("BillingState").HasMaxLength(100);
            address.Property(a => a.ZipCode).HasColumnName("BillingZipCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("BillingCountry").HasMaxLength(100);
        });

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.RegistrationDate)
            .IsRequired();

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
