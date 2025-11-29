using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Queries.ProductCategories;

public class GetProductCategoryByIdQueryHandler : IQueryHandler<GetProductCategoryByIdQuery, ProductCategoryDto?>
{
    private readonly IApplicationDbContext _context;

    public GetProductCategoryByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductCategoryDto?> Handle(GetProductCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _context.ProductCategories
            .Where(c => c.Id == request.Id && !c.IsDeleted)
            .Select(c => new ProductCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ParentCategoryId = c.ParentCategoryId,
                IsActive = c.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        return category;
    }
}
