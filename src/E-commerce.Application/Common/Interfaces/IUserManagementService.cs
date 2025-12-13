namespace E_commerce.Application.Common.Interfaces;

public interface IUserManagementService
{
    Task<bool> AssignRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
    Task<bool> RemoveRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CreateRoleAsync(string roleName, CancellationToken cancellationToken = default);
    Task<List<string>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteRoleAsync(string roleName, CancellationToken cancellationToken = default);
    Task<bool> LockUserAsync(Guid userId, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default);
    Task<bool> UnlockUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserDetailsDto?> GetUserDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<UserDetailsDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
}

public class UserDetailsDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public List<string> Roles { get; set; } = new();
    public Guid? CustomerId { get; set; }
}
