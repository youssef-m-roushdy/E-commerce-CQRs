using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Payments;

public record CreatePaymentCommand(
    Guid OrderId,
    decimal Amount,
    string Currency,
    string PaymentMethod,
    string? PaymentGateway
) : ICommand<Guid>;
