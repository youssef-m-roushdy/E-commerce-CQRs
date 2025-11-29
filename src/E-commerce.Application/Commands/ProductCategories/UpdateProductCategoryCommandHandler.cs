using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.ProductCategories;

public class UpdateProductCategoryCommandHandler : ICommandHandler<UpdateProductCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (category == null)
            return false;

        category.UpdateDetails(request.Name, request.Description);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
