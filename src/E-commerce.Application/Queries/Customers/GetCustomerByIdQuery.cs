using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;

namespace E_commerce.Application.Queries.Customers;

public record GetCustomerByIdQuery(Guid Id) : IQuery<CustomerDto?>;
