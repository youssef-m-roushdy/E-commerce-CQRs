using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Queries.Carts;

public class GetCartByCustomerQueryHandler : IQueryHandler<GetCartByCustomerQuery, CartDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCartByCustomerQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto?> Handle(GetCartByCustomerQuery request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .Where(c => c.CustomerId == request.CustomerId && !c.IsDeleted)
            .Select(c => new CartDto
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                LastModified = c.LastModified,
                TotalAmount = c.GetTotal().Amount,
                TotalItems = c.GetTotalItems(),
                CartItems = c.CartItems.Select(ci => new CartItemDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.ProductName,
                    UnitPrice = ci.UnitPrice.Amount,
                    Quantity = ci.Quantity,
                    TotalPrice = ci.TotalPrice.Amount
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return cart;
    }
}
