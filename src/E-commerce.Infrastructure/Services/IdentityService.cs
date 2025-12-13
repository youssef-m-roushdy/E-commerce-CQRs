using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.Common.Models;
using E_commerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace E_commerce.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResult?> LoginAsync(string usernameOrEmail, string password, CancellationToken cancellationToken = default)
    {
        // Try to find by username first, then by email
        var user = await _userManager.FindByNameAsync(usernameOrEmail);
        if (user == null)
            user = await _userManager.FindByEmailAsync(usernameOrEmail);
        
        if (user == null)
            return null;

        var result = await _userManager.CheckPasswordAsync(user, password);
        if (!result)
            return null;

        return await GenerateAuthResultAsync(user);
    }

    public async Task<AuthResult?> RegisterAsync(
        string username,
        string email, 
        string password, 
        string firstName, 
        string lastName, 
        Guid customerId, 
        CancellationToken cancellationToken = default)
    {
        // Check if username or email already exists
        var existingByUsername = await _userManager.FindByNameAsync(username);
        if (existingByUsername != null)
            return null;

        var existingByEmail = await _userManager.FindByEmailAsync(email);
        if (existingByEmail != null)
            return null;

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            CustomerId = customerId,
            EmailConfirmed = true // Auto-confirm for now
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return null;

        return await GenerateAuthResultAsync(user);
    }

    public async Task<AuthResult?> RefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
        if (principal == null)
            return null;

        var email = principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
            return null;

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return null;

        return await GenerateAuthResultAsync(user);
    }

    public async Task<bool> RevokeTokenAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
    {
        // Try to find by username first, then by email
        var user = await _userManager.FindByNameAsync(usernameOrEmail);
        if (user == null)
            user = await _userManager.FindByEmailAsync(usernameOrEmail);
        
        if (user == null)
            return false;

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<string?> GenerateEmailVerificationTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return null;

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        return token;
    }

    public async Task<bool> VerifyEmailAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return false;

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return null;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return token;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return false;

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }

    public async Task<AuthResult?> GoogleLoginAsync(string email, string firstName, string lastName, string googleId, Guid? customerId = null, CancellationToken cancellationToken = default)
    {
        // Check if user exists
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            // Create new user for Google sign-in
            user = new ApplicationUser
            {
                UserName = email.Split('@')[0] + "_" + Guid.NewGuid().ToString().Substring(0, 8),
                Email = email,
                EmailConfirmed = true, // Google emails are already verified
                CustomerId = customerId
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                return null;

            // Add Google login
            await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", googleId, "Google"));
        }
        else
        {
            // Check if user already has Google login
            var logins = await _userManager.GetLoginsAsync(user);
            if (!logins.Any(l => l.LoginProvider == "Google" && l.ProviderKey == googleId))
            {
                // Link Google account to existing user
                await _userManager.AddLoginAsync(user, new UserLoginInfo("Google", googleId, "Google"));
            }
        }

        return await GenerateAuthResultAsync(user);
    }

    private async Task<AuthResult> GenerateAuthResultAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new("CustomerId", user.CustomerId?.ToString() ?? string.Empty)
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var accessToken = _tokenService.GenerateAccessToken(claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);
        await _userManager.UpdateAsync(user);

        return new AuthResult
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
        };
    }
}
