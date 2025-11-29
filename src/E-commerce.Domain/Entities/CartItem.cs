using E_commerce.Domain.ValueObjects;

namespace E_commerce.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid ProductId { get; private set; } // Reference to the Product in cart
    public string ProductName { get; private set; } // Current product name (updated from Product)
    public Money UnitPrice { get; private set; } // Current price per unit (updated from Product)
    public int Quantity { get; private set; } // Number of units customer wants to purchase
    public Money TotalPrice { get; private set; } // Calculated total (UnitPrice × Quantity)

    private CartItem() { } // EF Core

    public CartItem(Guid productId, string productName, Money unitPrice, int quantity)
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
