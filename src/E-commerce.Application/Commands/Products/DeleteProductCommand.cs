using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Products;

public record DeleteProductCommand(Guid Id) : ICommand<bool>;
