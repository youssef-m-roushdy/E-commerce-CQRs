using E_commerce.Application.Common.Interfaces;
using E_commerce.Domain.Entities;
using E_commerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Carts;

public class AddToCartCommandHandler : ICommandHandler<AddToCartCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddToCartCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId && !c.IsDeleted, cancellationToken);

        if (cart == null)
        {
            cart = new Cart(request.CustomerId);
            _context.Carts.Add(cart);
        }

        var unitPrice = new Money(request.UnitPrice, request.Currency);
        cart.AddItem(request.ProductId, request.ProductName, unitPrice, request.Quantity);

        await _context.SaveChangesAsync(cancellationToken);

        return cart.Id;
    }
}
