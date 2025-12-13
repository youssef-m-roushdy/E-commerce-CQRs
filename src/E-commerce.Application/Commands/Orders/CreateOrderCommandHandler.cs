using E_commerce.Application.Common.Interfaces;
using E_commerce.Domain.Entities;
using E_commerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Application.Commands.Orders;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public CreateOrderCommandHandler(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var shippingAddress = new Address(
            request.ShippingAddress.Street,
            request.ShippingAddress.City,
            request.ShippingAddress.State,
            request.ShippingAddress.ZipCode,
            request.ShippingAddress.Country
        );

        Address? billingAddress = null;
        if (request.BillingAddress != null)
        {
            billingAddress = new Address(
                request.BillingAddress.Street,
                request.BillingAddress.City,
                request.BillingAddress.State,
                request.BillingAddress.ZipCode,
                request.BillingAddress.Country
            );
        }

        var order = new Order(
            request.CustomerId,
            shippingAddress,
            billingAddress,
            request.Notes
        );

        foreach (var itemRequest in request.Items)
        {
            var unitPrice = new Money(itemRequest.UnitPrice, itemRequest.Currency);
            var totalPrice = unitPrice * itemRequest.Quantity;

            var orderItem = new OrderItem(
                itemRequest.ProductId,
                itemRequest.ProductName,
                unitPrice,
                itemRequest.Quantity
            );

            order.AddItem(orderItem);
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        // Get customer name for notification
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId, cancellationToken);
        
        var customerName = customer != null ? $"{customer.FirstName} {customer.LastName}" : "Unknown Customer";
        
        // Send notification to customer
        await _notificationService.SendToUserAsync(
            request.CustomerId.ToString(),
            "Your order has been created successfully",
            new { OrderId = order.Id, TotalAmount = order.TotalAmount.Amount },
            cancellationToken);
        
        // Send notification to staff
        await _notificationService.SendNewOrderNotificationAsync(
            order.Id,
            customerName,
            order.TotalAmount.Amount,
            cancellationToken);

        return order.Id;
    }
}
