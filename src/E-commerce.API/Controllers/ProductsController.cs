using E_commerce.Application.Commands.Products;
using E_commerce.Application.Queries.Products;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.API.Controllers;

public class ProductsController : BaseApiController
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await Mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var productId = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = productId }, productId);
    }
}
