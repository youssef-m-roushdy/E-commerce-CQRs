using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Queries.Orders;

public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IApplicationDbContext _context;

    public GetOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.Id == request.Id && !o.IsDeleted)
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
            .FirstOrDefaultAsync(cancellationToken);

        return order;
    }
}
