using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Products;

public class UpdateProductStatusCommandHandler : ICommandHandler<UpdateProductStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateProductStatusCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (product == null)
            return false;

        switch (request.Status.ToUpper())
        {
            case "DISCONTINUED":
                product.MarkAsDiscontinued();
                break;
            case "UNAVAILABLE":
                product.MarkAsUnavailable();
                break;
            case "PREORDER":
                product.MarkAsPreorder();
                break;
            case "BACKORDER":
                product.MarkAsBackorder();
                break;
            default:
                return false;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
