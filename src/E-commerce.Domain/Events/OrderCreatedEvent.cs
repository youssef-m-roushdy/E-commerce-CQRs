namespace E_commerce.Domain.Events;

/// <summary>
/// Event raised when a new order is created
/// </summary>
public class OrderCreatedEvent : BaseDomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public decimal TotalAmount { get; }

    public OrderCreatedEvent(Guid orderId, Guid customerId, decimal totalAmount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }
}
