using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Products;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    string Currency,
    int Stock,
    string Category,
    string? Sku,
    string? ImageUrl,
    int LowStockThreshold
) : ICommand<Guid>;
