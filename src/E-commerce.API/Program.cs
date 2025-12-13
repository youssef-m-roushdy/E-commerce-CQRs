using E_commerce.Application;
using E_commerce.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "E-Commerce API",
        Version = "v1",
        Description = "Clean Architecture E-Commerce API with CQRS pattern"
    });

    // Add JWT Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Fixed Window - General API rate limiting
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = builder.Configuration.GetValue<int>("RateLimiting:FixedWindow:PermitLimit");
        opt.Window = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("RateLimiting:FixedWindow:Window"));
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    // Sliding Window - More flexible rate limiting
    options.AddSlidingWindowLimiter("sliding", opt =>
    {
        opt.PermitLimit = builder.Configuration.GetValue<int>("RateLimiting:SlidingWindow:PermitLimit");
        opt.Window = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("RateLimiting:SlidingWindow:Window"));
        opt.SegmentsPerWindow = builder.Configuration.GetValue<int>("RateLimiting:SlidingWindow:SegmentsPerWindow");
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 3;
    });

    // Token Bucket - For burst traffic
    options.AddTokenBucketLimiter("token", opt =>
    {
        opt.TokenLimit = builder.Configuration.GetValue<int>("RateLimiting:TokenBucket:TokenLimit");
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("RateLimiting:TokenBucket:ReplenishmentPeriod"));
        opt.TokensPerPeriod = builder.Configuration.GetValue<int>("RateLimiting:TokenBucket:TokensPerPeriod");
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 5;
    });

    // Concurrency Limiter - Limit concurrent requests
    options.AddConcurrencyLimiter("concurrency", opt =>
    {
        opt.PermitLimit = builder.Configuration.GetValue<int>("RateLimiting:Concurrency:PermitLimit");
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });

    // Global rate limiter per IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 200,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 5
            }));

    // Customize rejection response
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
            
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests",
                message = "Rate limit exceeded. Please try again later.",
                retryAfterSeconds = (int)retryAfter.TotalSeconds
            }, cancellationToken: token);
        }
        else
        {
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests",
                message = "Rate limit exceeded. Please try again later."
            }, cancellationToken: token);
        }
    };
});

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Commerce API V1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
