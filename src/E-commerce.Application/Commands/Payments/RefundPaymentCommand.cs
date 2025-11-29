using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Payments;

public record RefundPaymentCommand(Guid PaymentId) : ICommand<bool>;
