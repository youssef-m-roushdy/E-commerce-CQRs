using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;

namespace E_commerce.Application.Queries.Orders;

public record GetOrdersByCustomerQuery(Guid CustomerId) : IQuery<List<OrderDto>>;
