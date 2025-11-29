using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Carts;

public record ClearCartCommand(Guid CustomerId) : ICommand<bool>;
