using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Payments;

public record FailPaymentCommand(Guid PaymentId) : ICommand<bool>;

public class FailPaymentCommandHandler : ICommandHandler<FailPaymentCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public FailPaymentCommandHandler(
        IApplicationDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<bool> Handle(FailPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment == null)
            return false;

        // Get order to send notification
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == payment.OrderId, cancellationToken);

        if (order == null)
            return false;

        payment.MarkAsFailed();
        await _context.SaveChangesAsync(cancellationToken);

        // Send notification to customer
        await _notificationService.SendPaymentNotificationAsync(
            order.CustomerId.ToString(),
            payment.Id,
            "failed",
            payment.Amount.Amount,
            cancellationToken
        );

        return true;
    }
}
