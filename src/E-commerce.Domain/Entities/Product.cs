using E_commerce.Domain.ValueObjects;
using E_commerce.Domain.Enums;

namespace E_commerce.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; private set; } // Product name (e.g., "iPhone 15 Pro")
    public string Description { get; private set; } // Detailed product description
    public Money Price { get; private set; } // Product price with currency (Value Object)
    public int Stock { get; private set; } // Current stock quantity available
    public StockStatus Status { get; private set; } // Stock availability status (IN_STOCK, OUT_OF_STOCK, etc.)
    public string Category { get; private set; } // Product category name
    public string? ImageUrl { get; private set; } // URL to product image (optional)
    public string? Sku { get; private set; } // Stock Keeping Unit - unique product code (optional)
    public int LowStockThreshold { get; private set; } // Quantity threshold to trigger low stock warning (default: 10)

    private Product() { } // EF Core

    public Product(string name, string description, Money price, int stock, string category, string? sku = null, string? imageUrl = null, int lowStockThreshold = 10)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));
        if (stock < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(stock));

        Name = name;
        Description = description ?? string.Empty;
        Price = price ?? throw new ArgumentNullException(nameof(price));
        Stock = stock;
        Category = category ?? throw new ArgumentNullException(nameof(category));
        Sku = sku;
        ImageUrl = imageUrl;
        LowStockThreshold = lowStockThreshold;
        Status = CalculateStockStatus();
    }

    public void UpdateDetails(string name, string description, Money price, string category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
        Price = price ?? throw new ArgumentNullException(nameof(price));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        UpdateTimestamp();
    }

    public void UpdatePrice(Money newPrice)
    {
        Price = newPrice ?? throw new ArgumentNullException(nameof(newPrice));
        UpdateTimestamp();
    }

    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock cannot be negative", nameof(quantity));

        Stock = quantity;
        Status = CalculateStockStatus();
        UpdateTimestamp();
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        Stock += quantity;
        Status = CalculateStockStatus();
        UpdateTimestamp();
    }

    public void UpdateImage(string imageUrl)
    {
        ImageUrl = imageUrl;
        UpdateTimestamp();
    }

    public bool HasStock(int quantity) => Stock >= quantity;

    public void ReduceStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (!HasStock(quantity))
            throw new InvalidOperationException($"Insufficient stock. Available: {Stock}, Requested: {quantity}");
        
        Stock -= quantity;
        Status = CalculateStockStatus();
        UpdateTimestamp();
    }

    public void MarkAsDiscontinued()
    {
        Status = StockStatus.DISCONTINUED;
        UpdateTimestamp();
    }

    public void MarkAsUnavailable()
    {
        Status = StockStatus.UNAVAILABLE;
        UpdateTimestamp();
    }

    public void MarkAsPreorder()
    {
        Status = StockStatus.PREORDER;
        UpdateTimestamp();
    }

    public void MarkAsBackorder()
    {
        Status = StockStatus.BACKORDER;
        UpdateTimestamp();
    }

    public bool IsAvailableForPurchase() => 
        Status == StockStatus.IN_STOCK || 
        Status == StockStatus.LOW_STOCK || 
        Status == StockStatus.PREORDER || 
        Status == StockStatus.BACKORDER;

    public bool IsOutOfStock() => Status == StockStatus.OUT_OF_STOCK;

    public bool IsLowStock() => Status == StockStatus.LOW_STOCK;

    private StockStatus CalculateStockStatus()
    {
        // Don't auto-calculate if manually set to special statuses
        if (Status == StockStatus.DISCONTINUED || 
            Status == StockStatus.UNAVAILABLE || 
            Status == StockStatus.PREORDER || 
            Status == StockStatus.BACKORDER)
        {
            return Status;
        }

        if (Stock == 0)
            return StockStatus.OUT_OF_STOCK;
        
        if (Stock <= LowStockThreshold)
            return StockStatus.LOW_STOCK;
        
        return StockStatus.IN_STOCK;
    }
}
