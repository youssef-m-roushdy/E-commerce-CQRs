using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.Common.Models;
using Microsoft.Extensions.Options;
using Stripe;

namespace E_commerce.Infrastructure.Services;

/// <summary>
/// Stripe payment service implementation
/// </summary>
public class StripePaymentService : IPaymentService
{
    private readonly StripeSettings _stripeSettings;

    public StripePaymentService(IOptions<StripeSettings> stripeSettings)
    {
        _stripeSettings = stripeSettings.Value;
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }

    public async Task<string> CreatePaymentIntentAsync(
        decimal amount, 
        string currency, 
        string customerId, 
        Dictionary<string, string>? metadata = null, 
        CancellationToken cancellationToken = default)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), // Stripe uses cents
            Currency = currency.ToLower(),
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
            Metadata = metadata ?? new Dictionary<string, string>(),
            Description = $"Order payment for customer {customerId}"
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

        return paymentIntent.Id;
    }

    public async Task<bool> ConfirmPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.ConfirmAsync(paymentIntentId, cancellationToken: cancellationToken);

            return paymentIntent.Status == "succeeded" || paymentIntent.Status == "processing";
        }
        catch (StripeException)
        {
            return false;
        }
    }

    public async Task<bool> CancelPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.CancelAsync(paymentIntentId, cancellationToken: cancellationToken);

            return paymentIntent.Status == "canceled";
        }
        catch (StripeException)
        {
            return false;
        }
    }

    public async Task<string> CreateRefundAsync(string paymentIntentId, decimal? amount = null, CancellationToken cancellationToken = default)
    {
        var options = new RefundCreateOptions
        {
            PaymentIntent = paymentIntentId
        };

        if (amount.HasValue)
        {
            options.Amount = (long)(amount.Value * 100); // Convert to cents
        }

        var service = new RefundService();
        var refund = await service.CreateAsync(options, cancellationToken: cancellationToken);

        return refund.Id;
    }

    public async Task<Application.Common.Interfaces.PaymentDetails?> GetPaymentDetailsAsync(
        string paymentIntentId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);

            return new Application.Common.Interfaces.PaymentDetails
            {
                Id = paymentIntent.Id,
                Status = paymentIntent.Status,
                Amount = paymentIntent.Amount / 100m, // Convert from cents
                Currency = paymentIntent.Currency,
                CustomerId = paymentIntent.Metadata.TryGetValue("customerId", out var custId) ? custId : null,
                Metadata = paymentIntent.Metadata,
                Created = paymentIntent.Created
            };
        }
        catch (StripeException)
        {
            return null;
        }
    }

    public bool VerifyWebhookSignature(string payload, string signature)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                signature,
                _stripeSettings.WebhookSecret
            );

            return stripeEvent != null;
        }
        catch (StripeException)
        {
            return false;
        }
    }
}
