using E_commerce.Application.Common.Interfaces;

namespace E_commerce.Application.Commands.Orders;

public record CancelOrderCommand(Guid OrderId) : ICommand<bool>;
