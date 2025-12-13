using E_commerce.Application.Commands.Customers;
using E_commerce.Application.Queries.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace E_commerce.API.Controllers;

[Authorize]
[EnableRateLimiting("sliding")]
public class CustomersController : BaseApiController
{
    /// <summary>
    /// Get all customers (Admin, Manager, Support)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetCustomersQuery();
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get customer by ID (Admin, Manager, Support)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetCustomerByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Customer with ID {id} not found" });

        return Ok(result);
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command, CancellationToken cancellationToken)
    {
        var customerId = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = customerId }, new { id = customerId });
    }

    /// <summary>
    /// Update customer details (Admin, Manager, Support)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager,Support")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.FirstName,
            request.LastName,
            request.PhoneNumber
        );

        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Customer with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Delete a customer (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteCustomerCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Customer with ID {id} not found" });

        return NoContent();
    }
}

// Request DTOs
public record UpdateCustomerRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber
);
