using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;

namespace E_commerce.Application.Queries.Products;

public record GetProductsByCategoryQuery(string Category) : IQuery<List<ProductDto>>;
