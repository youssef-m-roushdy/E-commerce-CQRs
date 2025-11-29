using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Orders;

public record UpdateOrderStatusCommand(
    Guid OrderId,
    string Status
) : ICommand<bool>;
