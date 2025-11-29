using E_commerce.Domain.ValueObjects;

namespace E_commerce.Domain.Entities;

public class Cart : BaseEntity
{
    public Guid CustomerId { get; private set; } // Foreign key to Customer who owns this cart
    public DateTime LastModified { get; private set; } // When cart was last updated (UTC) - for abandoned cart tracking
    
    private readonly List<CartItem> _cartItems = new();
    public IReadOnlyCollection<CartItem> CartItems => _cartItems.AsReadOnly();

    private Cart() { } // EF Core

    public Cart(Guid customerId)
    {
        CustomerId = customerId;
        LastModified = DateTime.UtcNow;
    }

    public void AddItem(Guid productId, string productName, Money unitPrice, int quantity)
    {
        var existingItem = _cartItems.FirstOrDefault(i => i.ProductId == productId);
        
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var newItem = new CartItem(productId, productName, unitPrice, quantity);
            _cartItems.Add(newItem);
        }
        
        LastModified = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void RemoveItem(Guid cartItemId)
    {
        var item = _cartItems.FirstOrDefault(i => i.Id == cartItemId);
        if (item != null)
        {
            _cartItems.Remove(item);
            LastModified = DateTime.UtcNow;
            UpdateTimestamp();
        }
    }

    public void UpdateItemQuantity(Guid cartItemId, int quantity)
    {
        var item = _cartItems.FirstOrDefault(i => i.Id == cartItemId);
        if (item == null)
            throw new InvalidOperationException("Cart item not found");
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        item.UpdateQuantity(quantity);
        LastModified = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void Clear()
    {
        _cartItems.Clear();
        LastModified = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public Money GetTotal()
    {
        if (!_cartItems.Any())
            return Money.Zero();

        return _cartItems
            .Select(i => i.TotalPrice)
            .Aggregate((acc, price) => acc + price);
    }

    public bool IsEmpty() => !_cartItems.Any();
    
    public int GetTotalItems() => _cartItems.Sum(i => i.Quantity);
}