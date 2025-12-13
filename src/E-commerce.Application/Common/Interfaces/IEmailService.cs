namespace E_commerce.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
    Task SendVerificationEmailAsync(string to, string userName, string verificationToken, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string to, string userName, string resetToken, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string to, string userName, CancellationToken cancellationToken = default);
    Task SendOrderConfirmationEmailAsync(string to, string userName, string orderId, decimal totalAmount, CancellationToken cancellationToken = default);
}
