using Microsoft.AspNetCore.Identity;

namespace E_commerce.Infrastructure.Identity;

/// <summary>
/// Application user for authentication - extends IdentityUser
/// Links to Customer entity for business data
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? CustomerId { get; set; } // Link to Customer entity in Domain (nullable for admin users)
    public string? RefreshToken { get; set; } // For JWT refresh token storage
    public DateTime? RefreshTokenExpiryTime { get; set; } // When refresh token expires
    public DateTime CreatedAt { get; set; } // When user account was created
    public DateTime? LastLoginAt { get; set; } // Last login timestamp

    public ApplicationUser()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
}
