using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Products;

public record UpdateProductPriceCommand(
    Guid Id,
    decimal Price,
    string Currency
) : ICommand<bool>;
