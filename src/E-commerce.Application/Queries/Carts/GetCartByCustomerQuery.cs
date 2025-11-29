using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;

namespace E_commerce.Application.Queries.Carts;

public record GetCartByCustomerQuery(Guid CustomerId) : IQuery<CartDto?>;
