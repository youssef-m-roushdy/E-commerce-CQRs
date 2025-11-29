using E_commerce.Application.Common.Interfaces;
using E_commerce.Domain.Entities;
using E_commerce.Domain.Enums;
using E_commerce.Domain.ValueObjects;

namespace E_commerce.Application.Commands.Payments;

public class CreatePaymentCommandHandler : ICommandHandler<CreatePaymentCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreatePaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var amount = new Money(request.Amount, request.Currency);
        
        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var paymentMethod))
        {
            throw new ArgumentException($"Invalid payment method: {request.PaymentMethod}");
        }

        var payment = new Payment(
            request.OrderId,
            amount,
            paymentMethod,
            request.PaymentGateway
        );

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return payment.Id;
    }
}
