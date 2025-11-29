using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Orders;

public class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public CancelOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted, cancellationToken);

        if (order == null)
            return false;

        order.Cancel();
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
