using E_commerce.Application.Common.Interfaces;
using E_commerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Orders;

public class UpdateOrderStatusCommandHandler : ICommandHandler<UpdateOrderStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public UpdateOrderStatusCommandHandler(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted, cancellationToken);

        if (order == null)
            return false;

        switch (request.Status.ToUpper())
        {
            case "PROCESSING":
                order.MarkAsProcessing();
                break;
            case "SHIPPED":
                order.MarkAsShipped();
                break;
            case "DELIVERED":
                order.MarkAsDelivered();
                break;
            default:
                return false;
        }

        await _context.SaveChangesAsync(cancellationToken);
        
        // Send real-time notification to customer
        await _notificationService.SendOrderStatusUpdateAsync(
            order.CustomerId.ToString(),
            order.Id,
            request.Status,
            cancellationToken);
        
        return true;
    }
}
