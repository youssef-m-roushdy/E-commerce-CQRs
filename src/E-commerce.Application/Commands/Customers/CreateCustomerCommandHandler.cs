using E_commerce.Application.Common.Interfaces;
using E_commerce.Domain.Entities;
using E_commerce.Domain.ValueObjects;

namespace E_commerce.Application.Commands.Customers;

public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var email = new Email(request.Email);
        
        var customer = new Customer(
            request.FirstName,
            request.LastName,
            email,
            request.PhoneNumber
        );

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return customer.Id;
    }
}
