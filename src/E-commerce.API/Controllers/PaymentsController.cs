using E_commerce.Application.Commands.Payments;
using E_commerce.Application.Queries.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.API.Controllers;

[Authorize]
public class PaymentsController : BaseApiController
{
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
    /// Complete a payment (mark as completed with transaction ID)
    /// </summary>
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompletePaymentRequest request, CancellationToken cancellationToken)
    {
        var command = new CompletePaymentCommand(id, request.TransactionId);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Payment with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Refund a payment
    /// </summary>
    [HttpPost("{id}/refund")]
    public async Task<IActionResult> Refund(Guid id, CancellationToken cancellationToken)
    {
        var command = new RefundPaymentCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Payment with ID {id} not found or cannot be refunded" });

        return NoContent();
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
