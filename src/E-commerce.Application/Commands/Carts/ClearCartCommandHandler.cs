using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Carts;

public class ClearCartCommandHandler : ICommandHandler<ClearCartCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ClearCartCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId && !c.IsDeleted, cancellationToken);

        if (cart == null)
            return false;

        cart.Clear();
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
