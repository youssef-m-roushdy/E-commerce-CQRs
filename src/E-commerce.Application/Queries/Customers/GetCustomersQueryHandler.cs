using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Queries.Customers;

public class GetCustomersQueryHandler : IQueryHandler<GetCustomersQuery, List<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CustomerDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Customers
            .Where(c => !c.IsDeleted)
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
            .ToListAsync(cancellationToken);
    }
}
