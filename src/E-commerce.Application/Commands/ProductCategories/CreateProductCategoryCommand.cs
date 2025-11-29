using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.ProductCategories;

public record CreateProductCategoryCommand(
    string Name,
    string Description,
    Guid? ParentCategoryId,
    string? ImageUrl
) : ICommand<Guid>;
