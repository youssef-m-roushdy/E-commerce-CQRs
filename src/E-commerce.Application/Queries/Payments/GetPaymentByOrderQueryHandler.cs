using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Queries.Payments;

public class GetPaymentByOrderQueryHandler : IQueryHandler<GetPaymentByOrderQuery, PaymentDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentByOrderQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentDto?> Handle(GetPaymentByOrderQuery request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Where(p => p.OrderId == request.OrderId && !p.IsDeleted)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                OrderId = p.OrderId,
                Amount = p.Amount.Amount,
                PaymentDate = p.PaymentDate,
                Status = p.Status.ToString(),
                Method = p.Method.ToString(),
                TransactionId = p.TransactionId,
                PaymentGateway = p.PaymentGateway
            })
            .FirstOrDefaultAsync(cancellationToken);

        return payment;
    }
}
