using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Products;

public record UpdateProductStockCommand(
    Guid Id,
    int Stock
) : ICommand<bool>;
