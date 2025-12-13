namespace E_commerce.Application.Common.Interfaces;

/// <summary>
/// Service for sending real-time notifications via SignalR
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send notification to a specific user
    /// </summary>
    Task SendToUserAsync(string userId, string message, object? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send notification to users with specific role
    /// </summary>
    Task SendToRoleAsync(string role, string message, object? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send notification to all connected users
    /// </summary>
    Task SendToAllAsync(string message, object? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send notification to a specific group
    /// </summary>
    Task SendToGroupAsync(string groupName, string message, object? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send order status update notification
    /// </summary>
    Task SendOrderStatusUpdateAsync(string userId, Guid orderId, string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send payment status notification
    /// </summary>
    Task SendPaymentNotificationAsync(string userId, Guid paymentId, string status, decimal amount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send product stock alert to admins and managers
    /// </summary>
    Task SendStockAlertAsync(Guid productId, string productName, int currentStock, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send new order notification to staff
    /// </summary>
    Task SendNewOrderNotificationAsync(Guid orderId, string customerName, decimal totalAmount, CancellationToken cancellationToken = default);
}
