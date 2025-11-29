using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Carts;

public record UpdateCartItemCommand(
    Guid CustomerId,
    Guid CartItemId,
    int Quantity
) : ICommand<bool>;
