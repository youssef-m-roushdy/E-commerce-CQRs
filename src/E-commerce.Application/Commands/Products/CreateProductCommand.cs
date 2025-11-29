using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Products;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string Category
) : ICommand<Guid>;
