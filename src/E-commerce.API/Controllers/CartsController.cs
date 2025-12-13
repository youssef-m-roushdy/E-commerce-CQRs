using E_commerce.Application.Commands.Carts;
using E_commerce.Application.Queries.Carts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace E_commerce.API.Controllers;

[Authorize]
[EnableRateLimiting("token")]
public class CartsController : BaseApiController
{
    /// <summary>
    /// Get cart by customer ID
    /// </summary>
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        var query = new GetCartByCustomerQuery(customerId);
        var result = await Mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Cart for customer {customerId} not found" });

        return Ok(result);
    }

    /// <summary>
    /// Add item to cart
    /// </summary>
    [HttpPost("items")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request, CancellationToken cancellationToken)
    {
        var command = new AddToCartCommand(
            request.CustomerId,
            request.ProductId,
            request.ProductName,
            request.UnitPrice,
            request.Currency,
            request.Quantity
        );

        var cartItemId = await Mediator.Send(command, cancellationToken);
        return Ok(new { cartItemId });
    }

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    [HttpPut("items/{cartItemId}")]
    public async Task<IActionResult> UpdateCartItem(Guid cartItemId, [FromBody] UpdateCartItemRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCartItemCommand(
            request.CustomerId,
            cartItemId,
            request.Quantity
        );

        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Cart item {cartItemId} not found" });

        return NoContent();
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    [HttpDelete("items/{cartItemId}")]
    public async Task<IActionResult> RemoveFromCart(Guid cartItemId, [FromQuery] Guid customerId, CancellationToken cancellationToken)
    {
        var command = new RemoveFromCartCommand(customerId, cartItemId);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Cart item {cartItemId} not found" });

        return NoContent();
    }

    /// <summary>
    /// Clear all items from cart
    /// </summary>
    [HttpDelete("customer/{customerId}")]
    public async Task<IActionResult> ClearCart(Guid customerId, CancellationToken cancellationToken)
    {
        var command = new ClearCartCommand(customerId);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Cart for customer {customerId} not found" });

        return NoContent();
    }
}

// Request DTOs
public record AddToCartRequest(
    Guid CustomerId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity
);

public record UpdateCartItemRequest(
    Guid CustomerId,
    int Quantity
);
