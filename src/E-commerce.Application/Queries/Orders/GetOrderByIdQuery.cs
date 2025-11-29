using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;

namespace E_commerce.Application.Queries.Orders;

public record GetOrderByIdQuery(Guid Id) : IQuery<OrderDto?>;
