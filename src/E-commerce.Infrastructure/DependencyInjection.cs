using E_commerce.Application.Common.Interfaces;
using E_commerce.Application.Common.Models;
using E_commerce.Infrastructure.Identity;
using E_commerce.Infrastructure.Persistence;
using E_commerce.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace E_commerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Add Identity
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddSignInManager<SignInManager<ApplicationUser>>();

        // Configure JWT Settings
        var jwtSettings = configuration.GetSection("JwtSettings");
        services.Configure<JwtSettings>(jwtSettings);

        var jwtSettingsValue = jwtSettings.Get<JwtSettings>() ?? throw new InvalidOperationException("JWT settings not configured");

        // Add JWT Authentication with Google OAuth
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettingsValue.Issuer,
                ValidAudience = jwtSettingsValue.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettingsValue.Secret)),
                ClockSkew = TimeSpan.Zero
            };
        })
        .AddGoogle(options =>
        {
            var googleAuth = configuration.GetSection("GoogleAuth");
            options.ClientId = googleAuth["ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured");
            options.ClientSecret = googleAuth["ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret not configured");
            options.SaveTokens = true;
            
            // Request user profile information
            options.Scope.Add("profile");
            options.Scope.Add("email");
        });

        // Configure Email Settings
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        // Configure Cloudinary Settings
        services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));

        // Register Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();

        return services;
    }
}
