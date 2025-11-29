namespace E_commerce.Domain.Events;

/// <summary>
/// Event raised when product stock changes
/// </summary>
public class ProductStockChangedEvent : BaseDomainEvent
{
    public Guid ProductId { get; }
    public int OldStock { get; }
    public int NewStock { get; }
    public string Reason { get; }

    public ProductStockChangedEvent(Guid productId, int oldStock, int newStock, string reason)
    {
        ProductId = productId;
        OldStock = oldStock;
        NewStock = newStock;
        Reason = reason;
    }
}
