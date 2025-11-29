using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Products;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Currency,
    string Category
) : ICommand<bool>;
