using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Products;

public class UpdateProductStockCommandHandler : ICommandHandler<UpdateProductStockCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private const int LowStockThreshold = 10;

    public UpdateProductStockCommandHandler(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<bool> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (product == null)
            return false;

        product.UpdateStock(request.Stock);

        await _context.SaveChangesAsync(cancellationToken);
        
        // Send low stock alert if stock is below threshold
        if (request.Stock <= LowStockThreshold)
        {
            await _notificationService.SendStockAlertAsync(
                product.Id,
                product.Name,
                request.Stock,
                cancellationToken);
        }

        return true;
    }
}
