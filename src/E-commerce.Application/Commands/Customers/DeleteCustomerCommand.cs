using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Customers;

public record DeleteCustomerCommand(Guid Id) : ICommand<bool>;
