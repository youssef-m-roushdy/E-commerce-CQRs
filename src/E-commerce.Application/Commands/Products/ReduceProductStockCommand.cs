using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Products;

public record ReduceProductStockCommand(
    Guid Id,
    int Quantity
) : ICommand<bool>;
