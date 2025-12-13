using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace E_commerce.Application.Commands.Payments;

public class CompletePaymentCommandHandler : ICommandHandler<CompletePaymentCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public CompletePaymentCommandHandler(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<bool> Handle(CompletePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId && !p.IsDeleted, cancellationToken);

        if (payment == null)
            return false;

        payment.MarkAsCompleted(request.TransactionId);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Get order to retrieve customer ID for notification
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == payment.OrderId && !o.IsDeleted, cancellationToken);
        
        if (order != null)
        {
            // Send real-time payment notification to customer
            await _notificationService.SendPaymentNotificationAsync(
                order.CustomerId.ToString(),
                payment.Id,
                "Completed",
                payment.Amount.Amount,
                cancellationToken);
        }

        return true;
    }
}
