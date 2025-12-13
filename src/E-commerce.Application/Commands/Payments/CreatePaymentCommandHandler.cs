using E_commerce.Application.Common.Interfaces;
using E_commerce.Domain.Entities;
using E_commerce.Domain.Enums;
using E_commerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Payments;

public class CreatePaymentCommandHandler : ICommandHandler<CreatePaymentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentService _paymentService;

    public CreatePaymentCommandHandler(IApplicationDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    public async Task<Guid> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var amount = new Money(request.Amount, request.Currency);
        
        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var paymentMethod))
        {
            throw new ArgumentException($"Invalid payment method: {request.PaymentMethod}");
        }

        // Get order to validate and get customer ID
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new ArgumentException($"Order with ID {request.OrderId} not found");
        }

        // Create Stripe payment intent
        var metadata = new Dictionary<string, string>
        {
            { "orderId", request.OrderId.ToString() },
            { "customerId", order.CustomerId.ToString() }
        };

        var paymentIntentId = await _paymentService.CreatePaymentIntentAsync(
            request.Amount,
            request.Currency,
            order.CustomerId.ToString(),
            metadata,
            cancellationToken
        );

        var payment = new Payment(
            request.OrderId,
            amount,
            paymentMethod,
            request.PaymentGateway
        );

        // Store the Stripe payment intent ID as transaction ID
        payment.GetType().GetProperty("TransactionId")?.SetValue(payment, paymentIntentId);

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return payment.Id;
    }
}
