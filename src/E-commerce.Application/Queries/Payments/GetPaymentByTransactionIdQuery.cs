using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Queries.Payments;

public record GetPaymentByTransactionIdQuery(string TransactionId) : IQuery<PaymentDto?>;

public class GetPaymentByTransactionIdQueryHandler : IQueryHandler<GetPaymentByTransactionIdQuery, PaymentDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentByTransactionIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentDto?> Handle(GetPaymentByTransactionIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.TransactionId == request.TransactionId, cancellationToken);

        if (payment == null)
            return null;

        return new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount.Amount,
            PaymentDate = payment.PaymentDate,
            Status = payment.Status.ToString(),
            Method = payment.Method.ToString(),
            TransactionId = payment.TransactionId,
            PaymentGateway = payment.PaymentGateway
        };
    }
}
