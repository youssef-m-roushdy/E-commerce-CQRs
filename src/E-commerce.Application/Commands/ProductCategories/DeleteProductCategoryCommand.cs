using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.ProductCategories;

public record DeleteProductCategoryCommand(Guid Id) : ICommand<bool>;
