namespace E_commerce.Application.Common.Interfaces;

/// <summary>
/// Service for processing payments via Stripe
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Create a payment intent for an order
    /// </summary>
    Task<string> CreatePaymentIntentAsync(decimal amount, string currency, string customerId, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirm a payment intent
    /// </summary>
    Task<bool> ConfirmPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a payment intent
    /// </summary>
    Task<bool> CancelPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a refund for a payment
    /// </summary>
    Task<string> CreateRefundAsync(string paymentIntentId, decimal? amount = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payment details
    /// </summary>
    Task<PaymentDetails?> GetPaymentDetailsAsync(string paymentIntentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify webhook signature
    /// </summary>
    bool VerifyWebhookSignature(string payload, string signature);
}

public class PaymentDetails
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
    public DateTime Created { get; set; }
}
