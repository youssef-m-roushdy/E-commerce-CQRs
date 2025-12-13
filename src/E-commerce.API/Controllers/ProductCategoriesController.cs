using E_commerce.Application.Commands.ProductCategories;
using E_commerce.Application.Queries.ProductCategories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.API.Controllers;

[Authorize]
public class ProductCategoriesController : BaseApiController
{
    /// <summary>
    /// Get all product categories
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetProductCategoriesQuery();
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get product category by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductCategoryByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Product category with ID {id} not found" });

        return Ok(result);
    }

    /// <summary>
    /// Create a new product category
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCategoryRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProductCategoryCommand(
            request.Name,
            request.Description,
            request.ParentCategoryId,
            request.ImageUrl
        );

        var categoryId = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = categoryId }, new { id = categoryId });
    }

    /// <summary>
    /// Update product category details
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCategoryRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProductCategoryCommand(
            id,
            request.Name,
            request.Description
        );

        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Product category with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Delete a product category
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteProductCategoryCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Product category with ID {id} not found" });

        return NoContent();
    }
}

// Request DTOs
public record CreateProductCategoryRequest(
    string Name,
    string Description,
    Guid? ParentCategoryId,
    string? ImageUrl
);

public record UpdateProductCategoryRequest(
    string Name,
    string Description
);
