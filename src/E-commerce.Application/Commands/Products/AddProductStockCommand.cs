using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Products;

public record AddProductStockCommand(
    Guid Id,
    int Quantity
) : ICommand<bool>;
