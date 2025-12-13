using E_commerce.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
using E_commerce.Infrastructure.Hubs;

namespace E_commerce.Infrastructure.Services;

/// <summary>
/// Implementation of notification service using SignalR
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(string userId, string message, object? data = null, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("ReceiveNotification", new
            {
                Type = "Info",
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            }, cancellationToken);
    }

    public async Task SendToRoleAsync(string role, string message, object? data = null, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group($"role_{role}")
            .SendAsync("ReceiveNotification", new
            {
                Type = "Info",
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            }, cancellationToken);
    }

    public async Task SendToAllAsync(string message, object? data = null, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All
            .SendAsync("ReceiveNotification", new
            {
                Type = "Info",
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            }, cancellationToken);
    }

    public async Task SendToGroupAsync(string groupName, string message, object? data = null, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group(groupName)
            .SendAsync("ReceiveNotification", new
            {
                Type = "Info",
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            }, cancellationToken);
    }

    public async Task SendOrderStatusUpdateAsync(string userId, Guid orderId, string status, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("OrderStatusUpdate", new
            {
                Type = "OrderUpdate",
                Message = $"Your order status has been updated to: {status}",
                Data = new
                {
                    OrderId = orderId,
                    Status = status
                },
                Timestamp = DateTime.UtcNow
            }, cancellationToken);
    }

    public async Task SendPaymentNotificationAsync(string userId, Guid paymentId, string status, decimal amount, CancellationToken cancellationToken = default)
    {
        var message = status switch
        {
            "Completed" => $"Payment of ${amount:F2} completed successfully",
            "Failed" => $"Payment of ${amount:F2} failed",
            "Refunded" => $"Refund of ${amount:F2} processed",
            _ => $"Payment status: {status}"
        };

        await _hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("PaymentNotification", new
            {
                Type = "Payment",
                Message = message,
                Data = new
                {
                    PaymentId = paymentId,
                    Status = status,
                    Amount = amount
                },
                Timestamp = DateTime.UtcNow
            }, cancellationToken);
    }

    public async Task SendStockAlertAsync(Guid productId, string productName, int currentStock, CancellationToken cancellationToken = default)
    {
        var roles = new[] { "Admin", "Manager" };
        
        foreach (var role in roles)
        {
            await _hubContext.Clients
                .Group($"role_{role}")
                .SendAsync("StockAlert", new
                {
                    Type = "StockAlert",
                    Message = $"Low stock alert: {productName} (Stock: {currentStock})",
                    Data = new
                    {
                        ProductId = productId,
                        ProductName = productName,
                        CurrentStock = currentStock
                    },
                    Timestamp = DateTime.UtcNow
                }, cancellationToken);
        }
    }

    public async Task SendNewOrderNotificationAsync(Guid orderId, string customerName, decimal totalAmount, CancellationToken cancellationToken = default)
    {
        var roles = new[] { "Admin", "Manager", "Support" };
        
        foreach (var role in roles)
        {
            await _hubContext.Clients
                .Group($"role_{role}")
                .SendAsync("NewOrder", new
                {
                    Type = "NewOrder",
                    Message = $"New order from {customerName} - Total: ${totalAmount:F2}",
                    Data = new
                    {
                        OrderId = orderId,
                        CustomerName = customerName,
                        TotalAmount = totalAmount
                    },
                    Timestamp = DateTime.UtcNow
                }, cancellationToken);
        }
    }
}
