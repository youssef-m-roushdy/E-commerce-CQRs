using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Carts;

public record RemoveFromCartCommand(
    Guid CustomerId,
    Guid CartItemId
) : ICommand<bool>;
