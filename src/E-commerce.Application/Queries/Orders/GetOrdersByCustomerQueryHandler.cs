using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Queries.Orders;

public class GetOrdersByCustomerQueryHandler : IQueryHandler<GetOrdersByCustomerQuery, List<OrderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrdersByCustomerQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.CustomerId == request.CustomerId && !o.IsDeleted)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                OrderDate = o.OrderDate,
                Status = o.Status.ToString(),
                PaymentStatus = o.PaymentStatus.ToString(),
                Subtotal = o.Subtotal.Amount,
                Tax = o.Tax.Amount,
                ShippingCost = o.ShippingCost.Amount,
                TotalAmount = o.TotalAmount.Amount,
                ShippingAddress = new AddressDto
                {
                    Street = o.ShippingAddress.Street,
                    City = o.ShippingAddress.City,
                    State = o.ShippingAddress.State,
                    ZipCode = o.ShippingAddress.ZipCode,
                    Country = o.ShippingAddress.Country
                },
                Notes = o.Notes,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    UnitPrice = oi.UnitPrice.Amount,
                    Quantity = oi.Quantity,
                    TotalPrice = oi.TotalPrice.Amount
                }).ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
