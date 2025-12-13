using E_commerce.Application.Commands.Payments;
using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.Queries.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Stripe;

namespace E_commerce.API.Controllers;

[Authorize]
[EnableRateLimiting("concurrency")]
public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }
    /// <summary>
    /// Get payment by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPaymentByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Payment with ID {id} not found" });

        return Ok(result);
    }

    /// <summary>
    /// Get payment by order ID
    /// </summary>
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var query = new GetPaymentByOrderQuery(orderId);
        var result = await Mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Payment for order {orderId} not found" });

        return Ok(result);
    }

    /// <summary>
    /// Create a new payment
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePaymentCommand(
            request.OrderId,
            request.Amount,
            request.Currency,
            request.PaymentMethod,
            request.PaymentGateway
        );

        var paymentId = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = paymentId }, new { id = paymentId });
    }

    /// <summary>
    /// Complete a payment (Admin, Manager)
    /// </summary>
    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompletePaymentRequest request, CancellationToken cancellationToken)
    {
        var command = new CompletePaymentCommand(id, request.TransactionId);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Payment with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Refund a payment (Admin, Manager)
    /// </summary>
    [HttpPost("{id}/refund")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Refund(Guid id, CancellationToken cancellationToken)
    {
        var command = new RefundPaymentCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Payment with ID {id} not found or cannot be refunded" });

        return NoContent();
    }

    /// <summary>
    /// Create a Stripe payment intent
    /// </summary>
    [HttpPost("create-payment-intent")]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePaymentCommand(
            request.OrderId,
            request.Amount,
            request.Currency ?? "usd",
            request.PaymentMethod ?? "Card",
            "Stripe"
        );

        var paymentId = await Mediator.Send(command, cancellationToken);

        // Get the payment to retrieve the transaction ID (payment intent ID)
        var query = new GetPaymentByIdQuery(paymentId);
        var payment = await Mediator.Send(query, cancellationToken);

        if (payment == null)
            return BadRequest(new { message = "Failed to create payment" });

        // Get payment intent details to retrieve client secret
        var paymentDetails = payment.TransactionId != null 
            ? await _paymentService.GetPaymentDetailsAsync(payment.TransactionId, cancellationToken)
            : null;

        return Ok(new CreatePaymentIntentResponse
        {
            PaymentId = paymentId,
            PaymentIntentId = payment.TransactionId,
            ClientSecret = paymentDetails?.Id ?? string.Empty // In real implementation, you'd get the client_secret from Stripe
        });
    }

    /// <summary>
    /// Confirm a Stripe payment
    /// </summary>
    [HttpPost("confirm-payment")]
    public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request, CancellationToken cancellationToken)
    {
        var success = await _paymentService.ConfirmPaymentAsync(request.PaymentIntentId, cancellationToken);

        if (!success)
            return BadRequest(new { message = "Failed to confirm payment" });

        // Complete the payment in our system
        var command = new CompletePaymentCommand(request.PaymentId, request.PaymentIntentId);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Payment with ID {request.PaymentId} not found" });

        return Ok(new { message = "Payment confirmed successfully" });
    }

    /// <summary>
    /// Stripe webhook endpoint
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        if (!_paymentService.VerifyWebhookSignature(json, signature))
        {
            return BadRequest(new { message = "Invalid webhook signature" });
        }

        try
        {
            var stripeEvent = EventUtility.ParseEvent(json);

            // Handle the event
            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null)
                    {
                        // Find payment by transaction ID
                        var query = new GetPaymentByTransactionIdQuery(paymentIntent.Id);
                        var payment = await Mediator.Send(query, cancellationToken);

                        if (payment != null)
                        {
                            var command = new CompletePaymentCommand(payment.Id, paymentIntent.Id);
                            await Mediator.Send(command, cancellationToken);
                        }
                    }
                    break;

                case "payment_intent.payment_failed":
                    var failedIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (failedIntent != null)
                    {
                        // Find payment and mark as failed
                        var query = new GetPaymentByTransactionIdQuery(failedIntent.Id);
                        var payment = await Mediator.Send(query, cancellationToken);

                        if (payment != null)
                        {
                            var command = new FailPaymentCommand(payment.Id);
                            await Mediator.Send(command, cancellationToken);
                        }
                    }
                    break;

                case "charge.refunded":
                    var charge = stripeEvent.Data.Object as Charge;
                    if (charge != null && charge.PaymentIntentId != null)
                    {
                        var query = new GetPaymentByTransactionIdQuery(charge.PaymentIntentId);
                        var payment = await Mediator.Send(query, cancellationToken);

                        if (payment != null)
                        {
                            var command = new RefundPaymentCommand(payment.Id);
                            await Mediator.Send(command, cancellationToken);
                        }
                    }
                    break;

                default:
                    // Unhandled event type
                    break;
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

// Request DTOs
public record CreatePaymentRequest(
    Guid OrderId,
    decimal Amount,
    string Currency,
    string PaymentMethod,
    string? PaymentGateway
);

public record CompletePaymentRequest(string TransactionId);

public record CreatePaymentIntentRequest(
    Guid OrderId,
    decimal Amount,
    string? Currency = "usd",
    string? PaymentMethod = "Card"
);

public record CreatePaymentIntentResponse
{
    public Guid PaymentId { get; set; }
    public string PaymentIntentId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public record ConfirmPaymentRequest(
    Guid PaymentId,
    string PaymentIntentId
);
