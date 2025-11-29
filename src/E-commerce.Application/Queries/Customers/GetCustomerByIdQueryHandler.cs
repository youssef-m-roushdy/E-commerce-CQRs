using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Queries.Customers;

public class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers
            .Where(c => c.Id == request.Id && !c.IsDeleted)
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                FirstName = c.FirstName,
                LastName = c.LastName,
                FullName = c.FullName,
                Email = c.Email.Value,
                PhoneNumber = c.PhoneNumber,
                IsActive = c.IsActive,
                RegistrationDate = c.RegistrationDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        return customer;
    }
}
