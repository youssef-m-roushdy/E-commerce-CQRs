using E_commerce.Domain.ValueObjects;

namespace E_commerce.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid ProductId { get; private set; } // Reference to the Product being ordered
    public string ProductName { get; private set; } // Snapshot of product name at time of order (for history)
    public Money UnitPrice { get; private set; } // Price per unit at time of order (snapshot for history)
    public int Quantity { get; private set; } // Number of units ordered
    public Money TotalPrice { get; private set; } // Calculated total (UnitPrice × Quantity)

    private OrderItem() { } // EF Core

    public OrderItem(Guid productId, string productName, Money unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty", nameof(productName));

        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        Quantity = quantity;
        TotalPrice = unitPrice * quantity;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        Quantity = quantity;
        TotalPrice = UnitPrice * quantity;
        UpdateTimestamp();
    }
}
