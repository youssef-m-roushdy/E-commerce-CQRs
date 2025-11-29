using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Products;

public record UpdateProductImageCommand(
    Guid Id,
    string ImageUrl
) : ICommand<bool>;
