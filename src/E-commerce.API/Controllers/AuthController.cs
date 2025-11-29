using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.Commands.Customers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.API.Controllers;

public class AuthController : BaseApiController
{
    private readonly IIdentityService _identityService;

    public AuthController(IIdentityService identityService)
    {
        _identityService = identityService;
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
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            customerId,
            cancellationToken);

        if (authResult == null)
            return BadRequest(new { message = "Registration failed. Email may already be in use." });

        return Ok(authResult);
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var authResult = await _identityService.LoginAsync(request.Email, request.Password, cancellationToken);

        if (authResult == null)
            return Unauthorized(new { message = "Invalid email or password" });

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
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(email))
            return BadRequest(new { message = "Invalid user" });

        var result = await _identityService.RevokeTokenAsync(email, cancellationToken);

        if (!result)
            return BadRequest(new { message = "Token revocation failed" });

        return Ok(new { message = "Token revoked successfully" });
    }
}

public record LoginRequest(string Email, string Password);

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber
);

public record RefreshTokenRequest(string AccessToken, string RefreshToken);
