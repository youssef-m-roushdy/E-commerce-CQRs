# Integrating ASP.NET Core Identity with Customer Entity

## 🎯 Overview

There are **two approaches** to integrate Identity authentication with your Customer domain entity:

1. **Approach 1 (RECOMMENDED):** Keep Domain pure, link ApplicationUser to Customer
2. **Approach 2:** Make Customer inherit from IdentityUser (violates Clean Architecture)

---

## ✅ Approach 1: Separation of Concerns (RECOMMENDED)

### **Architecture:**
```
┌─────────────────────────────────────────┐
│     Infrastructure Layer                │
│  ┌───────────────────────────────────┐  │
│  │    ApplicationUser                │  │
│  │    (IdentityUser)                 │  │
│  │  - Email, Password, Roles, etc.   │  │
│  │  - CustomerId (FK) ────────────┐  │  │
│  └───────────────────────────────────┘  │
└──────────────────────────────────────|──┘
                                       │
┌──────────────────────────────────────|──┐
│     Domain Layer                     │  │
│  ┌───────────────────────────────────▼┐ │
│  │    Customer (BaseEntity)           │ │
│  │  - FirstName, LastName             │ │
│  │  - Email (VO), Phone               │ │
│  │  - Addresses                       │ │
│  │  - Business logic                  │ │
│  └────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

### **Why This is Better:**

✅ **Domain stays pure** - No framework dependencies in domain layer  
✅ **Separation of concerns** - Authentication vs Business logic  
✅ **Flexibility** - Can have users without customers (admins, staff)  
✅ **Testability** - Domain can be tested without Identity  
✅ **DDD principles** - Domain focuses only on business rules  

### **Implementation:**

#### 1. ApplicationUser (Infrastructure/Identity)
```csharp
using Microsoft.AspNetCore.Identity;

namespace E_commerce.Infrastructure.Identity;

/// <summary>
/// Handles authentication and authorization
/// Links to Customer for business data
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? CustomerId { get; set; } // Link to Customer (nullable for admin users)
    public string? RefreshToken { get; set; } // JWT refresh token
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation property (optional)
    public Customer? Customer { get; set; }

    public ApplicationUser()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
}
```

#### 2. Customer Entity (Domain) - UNCHANGED
```csharp
namespace E_commerce.Domain.Entities;

/// <summary>
/// Business entity for customer data
/// No authentication concerns
/// </summary>
public class Customer : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; } // Value Object
    public string? PhoneNumber { get; private set; }
    public Address? DefaultShippingAddress { get; private set; }
    public Address? DefaultBillingAddress { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime RegistrationDate { get; private set; }
    
    // Business logic methods...
}
```

#### 3. ApplicationDbContext (Infrastructure)
```csharp
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using E_commerce.Infrastructure.Identity;

namespace E_commerce.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Important! For Identity tables
        
        // Configure relationship
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Customer)
            .WithOne()
            .HasForeignKey<ApplicationUser>(u => u.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

#### 4. Registration Flow (Application Layer)
```csharp
// In your command handler
public async Task<Result> Handle(RegisterCustomerCommand request)
{
    // 1. Create Customer (business entity)
    var customer = new Customer(
        request.FirstName,
        request.LastName,
        new Email(request.Email),
        request.PhoneNumber
    );
    await _customerRepository.AddAsync(customer);
    
    // 2. Create ApplicationUser (authentication)
    var user = new ApplicationUser
    {
        UserName = request.Email,
        Email = request.Email,
        CustomerId = customer.Id, // Link them!
        EmailConfirmed = false
    };
    
    var result = await _userManager.CreateAsync(user, request.Password);
    if (!result.Succeeded)
    {
        // Handle error
    }
    
    await _userManager.AddToRoleAsync(user, "Customer");
    
    return Result.Success();
}
```

#### 5. Query Current User's Customer Data
```csharp
public class GetCurrentCustomerQueryHandler 
{
    private readonly ICurrentUserService _currentUserService; // Gets current user ID from JWT
    private readonly ApplicationDbContext _context;
    
    public async Task<CustomerDto> Handle(GetCurrentCustomerQuery request)
    {
        var userId = _currentUserService.UserId; // From JWT claims
        
        var user = await _context.Users
            .Include(u => u.Customer)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user?.Customer == null)
            throw new NotFoundException("Customer not found");
        
        return MapToDto(user.Customer);
    }
}
```

---

## ❌ Approach 2: Customer Inherits IdentityUser (NOT RECOMMENDED)

### **Why NOT Recommended:**

❌ **Violates Clean Architecture** - Domain depends on Identity framework  
❌ **Domain pollution** - Authentication concerns leak into domain  
❌ **Testing difficulty** - Need Identity mocks to test domain  
❌ **Flexibility loss** - All customers must be users  
❌ **Framework coupling** - Domain tied to Microsoft.Identity  

### **If You Still Want This (Not Recommended):**

```csharp
// DON'T DO THIS - shown for educational purposes only
using Microsoft.AspNetCore.Identity;

namespace E_commerce.Domain.Entities;

// ❌ BAD: Domain entity depends on framework
public class Customer : IdentityUser<Guid>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    // Loses email as Value Object
    // public Email Email { get; set; } // Can't use! IdentityUser has Email string
    
    // Inherited from IdentityUser:
    // - Id, UserName, Email, PasswordHash, etc.
    // - Lots of properties you don't need in domain
}
```

---

## 🎯 Recommended Setup Steps

### 1. Keep Current Customer Entity (Domain)
✅ Already done - no changes needed

### 2. Create ApplicationUser (Infrastructure)
```bash
# Already created at:
# src/E-commerce.Infrastructure/Identity/ApplicationUser.cs
```

### 3. Update ApplicationDbContext
```csharp
// Change from:
public class ApplicationDbContext : DbContext

// To:
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
```

### 4. Register Identity Services (Program.cs)
```csharp
// In E-commerce.API/Program.cs
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

### 5. Add Authentication/Authorization
```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
```

### 6. Create Migration
```bash
cd src/E-commerce.Infrastructure
dotnet ef migrations add AddIdentity --startup-project ../E-commerce.API
dotnet ef database update --startup-project ../E-commerce.API
```

---

## 📋 Summary

### **Use Approach 1 (ApplicationUser + Customer):**
- ✅ **Always** for Clean Architecture / DDD projects
- ✅ When domain needs to stay pure
- ✅ When you need flexibility (admins, staff users)
- ✅ For better testability

### **Avoid Approach 2 (Customer inherits IdentityUser):**
- ❌ Violates separation of concerns
- ❌ Couples domain to framework
- ❌ Harder to test and maintain

---

## 🔑 Key Concepts

**Authentication (Identity):** "Who are you?" - Login, passwords, tokens  
**Authorization:** "What can you do?" - Roles, permissions  
**Domain (Customer):** Business logic - Orders, addresses, profile  

**These are DIFFERENT concerns and should be separated!**

---

## 📚 Additional Resources

- [ASP.NET Core Identity Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Clean Architecture by Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [DDD: Separating Authentication from Domain](https://enterprisecraftsmanship.com/posts/domain-model-purity-vs-domain-model-completeness/)
