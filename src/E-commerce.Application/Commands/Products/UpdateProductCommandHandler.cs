using E_commerce.Application.Common.Interfaces;
using E_commerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Products;

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (product == null)
            return false;

        var price = new Money(request.Price, request.Currency);
        product.UpdateDetails(request.Name, request.Description, price, request.Category);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
