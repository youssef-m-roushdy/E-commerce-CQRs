using E_commerce.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Customers;

public class UpdateCustomerCommandHandler : ICommandHandler<UpdateCustomerCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken);

        if (customer == null)
            return false;

        customer.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.PhoneNumber
        );

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
