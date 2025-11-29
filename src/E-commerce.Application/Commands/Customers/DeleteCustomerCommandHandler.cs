using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Customers;

public class DeleteCustomerCommandHandler : ICommandHandler<DeleteCustomerCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (customer == null)
            return false;

        customer.MarkAsDeleted();
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
