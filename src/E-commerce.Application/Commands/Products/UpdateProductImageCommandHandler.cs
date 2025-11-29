using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Products;

public class UpdateProductImageCommandHandler : ICommandHandler<UpdateProductImageCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductImageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (product == null)
            return false;

        product.UpdateImage(request.ImageUrl);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
