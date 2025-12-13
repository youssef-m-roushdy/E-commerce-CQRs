using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.Commands.Customers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace E_commerce.API.Controllers;

[EnableRateLimiting("sliding")]
public class AuthController : BaseApiController
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;

    public AuthController(IIdentityService identityService, IEmailService emailService)
    {
        _identityService = identityService;
        _emailService = emailService;
    }

    /// <summary>
    /// Register a new customer account
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        // Create customer first
        var createCustomerCommand = new CreateCustomerCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber
        );

        var customerId = await Mediator.Send(createCustomerCommand, cancellationToken);

        // Create user account
        var authResult = await _identityService.RegisterAsync(
            request.Username,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            customerId,
            cancellationToken);

        if (authResult == null)
            return BadRequest(new { message = "Registration failed. Username or email may already be in use." });

        // Send welcome email
        await _emailService.SendWelcomeEmailAsync(request.Email, request.FirstName, cancellationToken);

        return Ok(authResult);
    }

    /// <summary>
    /// Login with username/email and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _identityService.LoginAsync(request.UsernameOrEmail, request.Password, cancellationToken);

        if (authResult == null)
            return Unauthorized(new { message = "Invalid username/email or password" });

        return Ok(authResult);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _identityService.RefreshTokenAsync(request.AccessToken, request.RefreshToken, cancellationToken);

        if (authResult == null)
            return Unauthorized(new { message = "Invalid token" });

        return Ok(authResult);
    }

    /// <summary>
    /// Revoke refresh token (logout)
    /// </summary>
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> RevokeToken(CancellationToken cancellationToken)
    {
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        
        if (string.IsNullOrEmpty(username))
            return BadRequest(new { message = "Invalid user" });

        var result = await _identityService.RevokeTokenAsync(username, cancellationToken);

        if (!result)
            return BadRequest(new { message = "Token revocation failed" });

        return Ok(new { message = "Token revoked successfully" });
    }

    /// <summary>
    /// Request email verification token
    /// </summary>
    [HttpPost("request-email-verification")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestEmailVerification([FromBody] EmailVerificationRequest request, CancellationToken cancellationToken)
    {
        var token = await _identityService.GenerateEmailVerificationTokenAsync(request.Email, cancellationToken);

        if (token == null)
            return NotFound(new { message = "User not found" });

        // Send verification email
        await _emailService.SendVerificationEmailAsync(request.Email, request.Email, token, cancellationToken);

        return Ok(new { message = "Verification email sent successfully" });
    }

    /// <summary>
    /// Verify email with token
    /// </summary>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await _identityService.VerifyEmailAsync(request.Email, request.Token, cancellationToken);

        if (!result)
            return BadRequest(new { message = "Email verification failed. Invalid or expired token." });

        return Ok(new { message = "Email verified successfully" });
    }

    /// <summary>
    /// Request password reset token
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var token = await _identityService.GeneratePasswordResetTokenAsync(request.Email, cancellationToken);

        if (token == null)
            return NotFound(new { message = "User not found" });

        // Send password reset email
        await _emailService.SendPasswordResetEmailAsync(request.Email, request.Email, token, cancellationToken);

        return Ok(new { message = "Password reset email sent successfully" });
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _identityService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, cancellationToken);

        if (!result)
            return BadRequest(new { message = "Password reset failed. Invalid or expired token." });

        return Ok(new { message = "Password reset successfully" });
    }
}

// Request DTOs
public record LoginRequest(string UsernameOrEmail, string Password);

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber
);

public record EmailVerificationRequest(string Email);

public record VerifyEmailRequest(string Email, string Token);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Email, string Token, string NewPassword);

public record RefreshTokenRequest(string AccessToken, string RefreshToken);
