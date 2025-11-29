using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Queries.Payments;

public class GetPaymentByIdQueryHandler : IQueryHandler<GetPaymentByIdQuery, PaymentDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments
            .Where(p => p.Id == request.Id && !p.IsDeleted)
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
