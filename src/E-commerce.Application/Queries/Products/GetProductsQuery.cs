using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;

namespace E_commerce.Application.Queries.Products;

public record GetProductsQuery : IQuery<List<ProductDto>>;
