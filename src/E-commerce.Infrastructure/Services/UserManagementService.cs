using E_commerce.Application.Common.Interfaces;
using E_commerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Infrastructure.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> AssignRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;

        // Create role if it doesn't exist
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }

        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }

    public async Task<bool> RemoveRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        return result.Succeeded;
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new List<string>();

        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList();
    }

    public async Task<bool> CreateRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
            return false;

        var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        return result.Succeeded;
    }

    public async Task<List<string>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync(cancellationToken);
        return roles;
    }

    public async Task<bool> DeleteRoleAsync(string roleName, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
            return false;

        var result = await _roleManager.DeleteAsync(role);
        return result.Succeeded;
    }

    public async Task<bool> LockUserAsync(Guid userId, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;

        user.LockoutEnabled = true;
        var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEnd ?? DateTimeOffset.UtcNow.AddYears(100));
        return result.Succeeded;
    }

    public async Task<bool> UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;

        var result = await _userManager.SetLockoutEndDateAsync(user, null);
        return result.Succeeded;
    }

    public async Task<UserDetailsDto?> GetUserDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDetailsDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            EmailConfirmed = user.EmailConfirmed,
            LockoutEnabled = user.LockoutEnabled,
            LockoutEnd = user.LockoutEnd,
            Roles = roles.ToList(),
            CustomerId = user.CustomerId
        };
    }

    public async Task<List<UserDetailsDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users.ToListAsync(cancellationToken);
        var userDetailsList = new List<UserDetailsDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDetailsList.Add(new UserDetailsDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                Roles = roles.ToList(),
                CustomerId = user.CustomerId
            });
        }

        return userDetailsList;
    }
}
