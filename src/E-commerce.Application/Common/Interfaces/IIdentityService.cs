using E_commerce.Application.Common.Models;

namespace E_commerce.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthResult?> LoginAsync(string usernameOrEmail, string password, CancellationToken cancellationToken = default);
    Task<AuthResult?> RegisterAsync(string username, string email, string password, string firstName, string lastName, Guid customerId, CancellationToken cancellationToken = default);
    Task<AuthResult?> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> RevokeTokenAsync(string usernameOrEmail, CancellationToken cancellationToken = default);
    Task<string?> GenerateEmailVerificationTokenAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> VerifyEmailAsync(string email, string token, CancellationToken cancellationToken = default);
    Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default);
}
