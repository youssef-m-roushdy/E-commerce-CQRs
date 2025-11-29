using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Queries.Products;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Where(p => p.Id == request.Id && !p.IsDeleted)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price.Amount,
                Currency = p.Price.Currency,
                Stock = p.Stock,
                Status = p.Status.ToString(),
                Category = p.Category,
                ImageUrl = p.ImageUrl,
                Sku = p.Sku,
                LowStockThreshold = p.LowStockThreshold,
                IsAvailableForPurchase = p.IsAvailableForPurchase()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return product;
    }
}
