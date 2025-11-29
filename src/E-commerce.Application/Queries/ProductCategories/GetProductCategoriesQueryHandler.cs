using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Queries.ProductCategories;

public class GetProductCategoriesQueryHandler : IQueryHandler<GetProductCategoriesQuery, List<ProductCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductCategoryDto>> Handle(GetProductCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.ProductCategories
            .Where(c => !c.IsDeleted)
            .Select(c => new ProductCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ParentCategoryId = c.ParentCategoryId,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
