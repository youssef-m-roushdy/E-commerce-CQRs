using E_commerce.Application.Commands.Products;
using E_commerce.Application.Queries.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.API.Controllers;

[Authorize]
public class ProductsController : BaseApiController
{
    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetProductsQuery();
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return Ok(result);
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{category}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCategory(string category, CancellationToken cancellationToken)
    {
        var query = new GetProductsByCategoryQuery(category);
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get available products (in stock and active)
    /// </summary>
    [HttpGet("available")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailable(CancellationToken cancellationToken)
    {
        var query = new GetAvailableProductsQuery();
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        var productId = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = productId }, new { id = productId });
    }

    /// <summary>
    /// Update product details
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.Currency,
            request.Category
        );

        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Update product price
    /// </summary>
    [HttpPatch("{id}/price")]
    public async Task<IActionResult> UpdatePrice(Guid id, [FromBody] UpdatePriceRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProductPriceCommand(id, request.Price, request.Currency);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Update product status
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProductStatusCommand(id, request.Status);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Update product image
    /// </summary>
    [HttpPatch("{id}/image")]
    public async Task<IActionResult> UpdateImage(Guid id, [FromBody] UpdateImageRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProductImageCommand(id, request.ImageUrl);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Update product stock
    /// </summary>
    [HttpPatch("{id}/stock")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] UpdateStockRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProductStockCommand(id, request.Stock);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Add stock to product
    /// </summary>
    [HttpPost("{id}/stock/add")]
    public async Task<IActionResult> AddStock(Guid id, [FromBody] AddStockRequest request, CancellationToken cancellationToken)
    {
        var command = new AddProductStockCommand(id, request.Quantity);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Reduce stock from product
    /// </summary>
    [HttpPost("{id}/stock/reduce")]
    public async Task<IActionResult> ReduceStock(Guid id, [FromBody] ReduceStockRequest request, CancellationToken cancellationToken)
    {
        var command = new ReduceProductStockCommand(id, request.Quantity);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return NoContent();
    }
}

// Request DTOs
public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    string Currency,
    string Category
);

public record UpdatePriceRequest(decimal Price, string Currency);
public record UpdateStatusRequest(string Status);
public record UpdateImageRequest(string ImageUrl);
public record UpdateStockRequest(int Stock);
public record AddStockRequest(int Quantity);
public record ReduceStockRequest(int Quantity);
