using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.ProductCategories;

public record UpdateProductCategoryCommand(
    Guid Id,
    string Name,
    string Description
) : ICommand<bool>;
