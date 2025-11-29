using E_commerce.Domain.Enums;
using E_commerce.Domain.ValueObjects;

namespace E_commerce.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; private set; } // Foreign key to Order this payment is for
    public Money Amount { get; private set; } // Payment amount (should match order total)
    public DateTime PaymentDate { get; private set; } // When payment was initiated (UTC)
    public PaymentStatus Status { get; private set; } // Payment status (PENDING, COMPLETED, FAILED, REFUNDED, etc.)
    public PaymentMethod Method { get; private set; } // How customer paid (CreditCard, PayPal, Stripe, etc.)
    public string? TransactionId { get; private set; } // Unique transaction ID from payment gateway (set on success)
    public string? PaymentGateway { get; private set; } // Payment gateway used (e.g., "Stripe", "PayPal")

    private Payment() { } // EF Core

    public Payment(Guid orderId, Money amount, PaymentMethod method, string? paymentGateway = null)
    {
        OrderId = orderId;
        Amount = amount ?? throw new ArgumentNullException(nameof(amount));
        Method = method;
        PaymentGateway = paymentGateway;
        PaymentDate = DateTime.UtcNow;
        Status = PaymentStatus.PENDING;
    }

    public void MarkAsCompleted(string transactionId)
    {
        if (Status != PaymentStatus.PENDING)
            throw new InvalidOperationException($"Cannot complete payment with status {Status}");
        
        Status = PaymentStatus.COMPLETED;
        TransactionId = transactionId;
        UpdateTimestamp();
    }

    public void MarkAsFailed()
    {
        if (Status != PaymentStatus.PENDING)
            throw new InvalidOperationException($"Cannot fail payment with status {Status}");
        
        Status = PaymentStatus.FAILED;
        UpdateTimestamp();
    }

    public void MarkAsRefunded()
    {
        if (Status != PaymentStatus.COMPLETED)
            throw new InvalidOperationException("Can only refund completed payments");
        
        Status = PaymentStatus.REFUNDED;
        UpdateTimestamp();
    }

    public void Cancel()
    {
        if (Status == PaymentStatus.COMPLETED || Status == PaymentStatus.REFUNDED)
            throw new InvalidOperationException($"Cannot cancel payment with status {Status}");
        
        Status = PaymentStatus.CANCELLED;
        UpdateTimestamp();
    }

    public bool IsSuccessful() => Status == PaymentStatus.COMPLETED;
}