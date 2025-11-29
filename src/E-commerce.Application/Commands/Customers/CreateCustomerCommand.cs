using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Customers;

public record CreateCustomerCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber
) : ICommand<Guid>;
