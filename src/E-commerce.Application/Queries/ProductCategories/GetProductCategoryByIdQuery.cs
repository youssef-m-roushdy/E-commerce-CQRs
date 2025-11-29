using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;

namespace E_commerce.Application.Queries.ProductCategories;

public record GetProductCategoryByIdQuery(Guid Id) : IQuery<ProductCategoryDto?>;
