using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;

namespace E_commerce.Application.Commands.Orders;

public record CreateOrderCommand(
    Guid CustomerId,
    AddressDto ShippingAddress,
    AddressDto? BillingAddress,
    string? Notes,
    List<OrderItemRequest> Items
) : ICommand<Guid>;

public record OrderItemRequest(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity
);
