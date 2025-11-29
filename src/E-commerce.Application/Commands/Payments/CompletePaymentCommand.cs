using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Payments;

public record CompletePaymentCommand(
    Guid PaymentId,
    string TransactionId
) : ICommand<bool>;
