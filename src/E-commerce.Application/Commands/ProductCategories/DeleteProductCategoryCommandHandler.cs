using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.ProductCategories;

public class DeleteProductCategoryCommandHandler : ICommandHandler<DeleteProductCategoryCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (category == null)
            return false;

        category.MarkAsDeleted();
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
