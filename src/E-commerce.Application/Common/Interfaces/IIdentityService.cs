using E_commerce.Application.Common.Models;

namespace E_commerce.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<AuthResult?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<AuthResult?> RegisterAsync(string email, string password, string firstName, string lastName, Guid customerId, CancellationToken cancellationToken = default);
    Task<AuthResult?> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> RevokeTokenAsync(string email, CancellationToken cancellationToken = default);
}
