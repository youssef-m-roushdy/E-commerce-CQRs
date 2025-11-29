using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Payments;

public class RefundPaymentCommandHandler : ICommandHandler<RefundPaymentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RefundPaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId && !p.IsDeleted, cancellationToken);

        if (payment == null)
            return false;

        payment.MarkAsRefunded();
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
