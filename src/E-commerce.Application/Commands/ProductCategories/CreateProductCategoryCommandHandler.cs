using E_commerce.Application.Common.Interfaces;
using E_commerce.Domain.Entities;

namespace E_commerce.Application.Commands.ProductCategories;

public class CreateProductCategoryCommandHandler : ICommandHandler<CreateProductCategoryCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new ProductCategory(
            request.Name,
            request.Description,
            request.ParentCategoryId,
            request.ImageUrl
        );

        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
