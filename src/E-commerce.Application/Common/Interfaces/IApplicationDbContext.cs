using E_commerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
