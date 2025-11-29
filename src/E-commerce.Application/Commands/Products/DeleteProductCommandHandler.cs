using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Products;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (product == null)
            return false;

        product.MarkAsDeleted();

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
