using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Carts;

public class RemoveFromCartCommandHandler : ICommandHandler<RemoveFromCartCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RemoveFromCartCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId && !c.IsDeleted, cancellationToken);

        if (cart == null)
            return false;

        cart.RemoveItem(request.CartItemId);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
