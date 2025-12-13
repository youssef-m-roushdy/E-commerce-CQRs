using E_commerce.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace E_commerce.API.Controllers;

[Authorize]
[EnableRateLimiting("sliding")]
public class UsersController : BaseApiController
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userManagementService.GetAllUsersAsync(cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    [HttpGet("{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userManagementService.GetUserDetailsAsync(userId, cancellationToken);

        if (user == null)
            return NotFound(new { message = $"User with ID {userId} not found" });

        return Ok(user);
    }

    /// <summary>
    /// Assign role to user (Admin only)
    /// </summary>
    [HttpPost("{userId}/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole(Guid userId, [FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.AssignRoleAsync(userId, request.RoleName, cancellationToken);

        if (!result)
            return BadRequest(new { message = $"Failed to assign role '{request.RoleName}' to user" });

        return Ok(new { message = $"Role '{request.RoleName}' assigned successfully" });
    }

    /// <summary>
    /// Remove role from user (Admin only)
    /// </summary>
    [HttpDelete("{userId}/roles/{roleName}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveRole(Guid userId, string roleName, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.RemoveRoleAsync(userId, roleName, cancellationToken);

        if (!result)
            return BadRequest(new { message = $"Failed to remove role '{roleName}' from user" });

        return Ok(new { message = $"Role '{roleName}' removed successfully" });
    }

    /// <summary>
    /// Get user roles
    /// </summary>
    [HttpGet("{userId}/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserRoles(Guid userId, CancellationToken cancellationToken)
    {
        var roles = await _userManagementService.GetUserRolesAsync(userId, cancellationToken);
        return Ok(new { userId, roles });
    }

    /// <summary>
    /// Lock user account (Admin only)
    /// </summary>
    [HttpPost("{userId}/lock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> LockUser(Guid userId, [FromBody] LockUserRequest request, CancellationToken cancellationToken)
    {
        DateTimeOffset? lockoutEnd = request.LockoutDays.HasValue 
            ? DateTimeOffset.UtcNow.AddDays(request.LockoutDays.Value) 
            : null;

        var result = await _userManagementService.LockUserAsync(userId, lockoutEnd, cancellationToken);

        if (!result)
            return BadRequest(new { message = "Failed to lock user account" });

        return Ok(new { message = "User account locked successfully" });
    }

    /// <summary>
    /// Unlock user account (Admin only)
    /// </summary>
    [HttpPost("{userId}/unlock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnlockUser(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.UnlockUserAsync(userId, cancellationToken);

        if (!result)
            return BadRequest(new { message = "Failed to unlock user account" });

        return Ok(new { message = "User account unlocked successfully" });
    }

    /// <summary>
    /// Get all roles (Admin only)
    /// </summary>
    [HttpGet("~/api/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken)
    {
        var roles = await _userManagementService.GetAllRolesAsync(cancellationToken);
        return Ok(roles);
    }

    /// <summary>
    /// Create new role (Admin only)
    /// </summary>
    [HttpPost("~/api/roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.CreateRoleAsync(request.RoleName, cancellationToken);

        if (!result)
            return BadRequest(new { message = $"Failed to create role '{request.RoleName}'. Role may already exist." });

        return Ok(new { message = $"Role '{request.RoleName}' created successfully" });
    }

    /// <summary>
    /// Delete role (Admin only)
    /// </summary>
    [HttpDelete("~/api/roles/{roleName}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRole(string roleName, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.DeleteRoleAsync(roleName, cancellationToken);

        if (!result)
            return NotFound(new { message = $"Role '{roleName}' not found" });

        return Ok(new { message = $"Role '{roleName}' deleted successfully" });
    }
}

// Request DTOs
public record AssignRoleRequest(string RoleName);

public record CreateRoleRequest(string RoleName);

public record LockUserRequest(int? LockoutDays); // null = permanent lock
