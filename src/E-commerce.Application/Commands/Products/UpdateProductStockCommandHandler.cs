using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Products;

public class UpdateProductStockCommandHandler : ICommandHandler<UpdateProductStockCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductStockCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (product == null)
            return false;

        product.UpdateStock(request.Stock);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
