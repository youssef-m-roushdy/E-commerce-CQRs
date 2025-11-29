using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Queries.Products;

public class GetAvailableProductsQueryHandler : IQueryHandler<GetAvailableProductsQuery, List<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAvailableProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> Handle(GetAvailableProductsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Products
            .Where(p => !p.IsDeleted && p.IsAvailableForPurchase())
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
            .ToListAsync(cancellationToken);
    }
}
