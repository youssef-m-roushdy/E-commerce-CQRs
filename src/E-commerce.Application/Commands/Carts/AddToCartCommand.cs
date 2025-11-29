using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Carts;

public record AddToCartCommand(
    Guid CustomerId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity
) : ICommand<Guid>;
