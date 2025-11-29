using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Products;

public class ReduceProductStockCommandHandler : ICommandHandler<ReduceProductStockCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ReduceProductStockCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ReduceProductStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (product == null)
            return false;

        product.ReduceStock(request.Quantity);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
