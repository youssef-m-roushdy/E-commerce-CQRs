using E_commerce.Domain.Enums;
using E_commerce.Domain.ValueObjects;

namespace E_commerce.Domain.Entities;

public class Order : BaseEntity
{
    public Guid CustomerId { get; private set; } // Foreign key to Customer who placed the order
    public DateTime OrderDate { get; private set; } // When the order was placed (UTC)
    public OrderStatus Status { get; private set; } // Current order status (PENDING, PROCESSING, SHIPPED, etc.)
    public PaymentStatus PaymentStatus { get; private set; } // Payment status (PENDING, COMPLETED, FAILED, etc.)
    public Money Subtotal { get; private set; } // Sum of all order items (before tax and shipping)
    public Money Tax { get; private set; } // Tax amount applied to order
    public Money ShippingCost { get; private set; } // Shipping/delivery cost
    public Money TotalAmount { get; private set; } // Final total (Subtotal + Tax + ShippingCost)
    public Address ShippingAddress { get; private set; } // Where to deliver the order (Value Object)
    public Address? BillingAddress { get; private set; } // Billing address (optional, defaults to shipping if not provided)
    public string? Notes { get; private set; } // Optional customer notes/instructions for the order
    
    private readonly List<OrderItem> _orderItems = new();
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    private Order() { } // EF Core

    public Order(Guid customerId, Address shippingAddress, Address? billingAddress = null, string? notes = null)
    {
        CustomerId = customerId;
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.PENDING;
        PaymentStatus = PaymentStatus.PENDING;
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
        BillingAddress = billingAddress;
        Notes = notes;
        Subtotal = Money.Zero();
        Tax = Money.Zero();
        ShippingCost = Money.Zero();
        TotalAmount = Money.Zero();
    }

    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.PENDING)
            throw new InvalidOperationException("Cannot add items to a non-pending order");

        _orderItems.Add(item);
        RecalculateTotals();
        UpdateTimestamp();
    }

    public void RemoveItem(Guid orderItemId)
    {
        if (Status != OrderStatus.PENDING)
            throw new InvalidOperationException("Cannot remove items from a non-pending order");

        var item = _orderItems.FirstOrDefault(i => i.Id == orderItemId);
        if (item != null)
        {
            _orderItems.Remove(item);
            RecalculateTotals();
            UpdateTimestamp();
        }
    }

    public void SetShippingCost(Money shippingCost)
    {
        ShippingCost = shippingCost ?? throw new ArgumentNullException(nameof(shippingCost));
        RecalculateTotals();
        UpdateTimestamp();
    }

    public void SetTax(Money tax)
    {
        Tax = tax ?? throw new ArgumentNullException(nameof(tax));
        RecalculateTotals();
        UpdateTimestamp();
    }

    public void MarkAsProcessing()
    {
        if (Status != OrderStatus.PENDING)
            throw new InvalidOperationException($"Cannot process order with status {Status}");
        
        Status = OrderStatus.PROCESSING;
        UpdateTimestamp();
    }

    public void MarkAsShipped()
    {
        if (Status != OrderStatus.PROCESSING)
            throw new InvalidOperationException($"Cannot ship order with status {Status}");
        
        Status = OrderStatus.SHIPPED;
        UpdateTimestamp();
    }

    public void MarkAsDelivered()
    {
        if (Status != OrderStatus.SHIPPED)
            throw new InvalidOperationException($"Cannot deliver order with status {Status}");
        
        Status = OrderStatus.DELIVERED;
        UpdateTimestamp();
    }

    public void Cancel()
    {
        if (Status == OrderStatus.DELIVERED || Status == OrderStatus.CANCELED)
            throw new InvalidOperationException($"Cannot cancel order with status {Status}");
        
        Status = OrderStatus.CANCELED;
        UpdateTimestamp();
    }

    public void MarkPaymentAsCompleted()
    {
        PaymentStatus = PaymentStatus.COMPLETED;
        UpdateTimestamp();
    }

    public void MarkPaymentAsFailed()
    {
        PaymentStatus = PaymentStatus.FAILED;
        UpdateTimestamp();
    }

    private void RecalculateTotals()
    {
        Subtotal = _orderItems
            .Select(i => i.TotalPrice)
            .Aggregate(Money.Zero(), (acc, price) => acc + price);
        
        TotalAmount = Subtotal + Tax + ShippingCost;
    }

    public bool CanBeCanceled() => Status == OrderStatus.PENDING || Status == OrderStatus.PROCESSING;
    
    public bool IsCompleted() => Status == OrderStatus.DELIVERED;
}