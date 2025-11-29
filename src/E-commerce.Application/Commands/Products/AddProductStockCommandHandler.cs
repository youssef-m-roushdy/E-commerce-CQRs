using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Products;

public class AddProductStockCommandHandler : ICommandHandler<AddProductStockCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public AddProductStockCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(AddProductStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (product == null)
            return false;

        product.AddStock(request.Quantity);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
