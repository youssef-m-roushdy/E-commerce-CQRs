using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Customers;

public record UpdateCustomerCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? PhoneNumber
) : ICommand<bool>;
