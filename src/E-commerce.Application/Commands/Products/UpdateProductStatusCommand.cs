using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Products;

public record UpdateProductStatusCommand(
    Guid Id,
    string Status
) : ICommand<bool>;
