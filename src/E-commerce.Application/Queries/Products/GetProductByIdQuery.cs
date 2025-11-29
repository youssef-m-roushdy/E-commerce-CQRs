using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Queries.Products;

public record GetProductByIdQuery(Guid Id) : IQuery<ProductDto?>;
