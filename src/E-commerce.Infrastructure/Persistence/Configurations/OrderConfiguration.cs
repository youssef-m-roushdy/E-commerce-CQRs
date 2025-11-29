using E_commerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_commerce.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.OrderDate)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(o => o.PaymentStatus)
            .IsRequired()
            .HasConversion<string>();

        builder.OwnsOne(o => o.Subtotal, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Subtotal")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("SubtotalCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(o => o.Tax, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Tax")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("TaxCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(o => o.ShippingCost, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("ShippingCost")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("ShippingCostCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(o => o.TotalAmount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("TotalAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("TotalAmountCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("ShippingStreet").HasMaxLength(200).IsRequired();
            address.Property(a => a.City).HasColumnName("ShippingCity").HasMaxLength(100).IsRequired();
            address.Property(a => a.State).HasColumnName("ShippingState").HasMaxLength(100).IsRequired();
            address.Property(a => a.ZipCode).HasColumnName("ShippingZipCode").HasMaxLength(20).IsRequired();
            address.Property(a => a.Country).HasColumnName("ShippingCountry").HasMaxLength(100).IsRequired();
        });

        builder.OwnsOne(o => o.BillingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("BillingStreet").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("BillingCity").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("BillingState").HasMaxLength(100);
            address.Property(a => a.ZipCode).HasColumnName("BillingZipCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("BillingCountry").HasMaxLength(100);
        });

        builder.Property(o => o.Notes)
            .HasMaxLength(1000);

        builder.HasMany(o => o.OrderItems)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(o => o.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasQueryFilter(o => !o.IsDeleted);

        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.OrderDate);
        builder.HasIndex(o => o.Status);
    }
}
